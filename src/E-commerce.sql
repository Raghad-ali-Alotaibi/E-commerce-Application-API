--------------------------------- Create Database

CREATE DATABASE ecommerce_sda;

--------------------------------- Create Category Table

CREATE TABLE category
(
    category_id SERIAL PRIMARY KEY,
    category_name VARCHAR(50) UNIQUE NOT NULL,
    category_slug VARCHAR(100) UNIQUE NOT NULL,
    category_description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

---------------------------- Insert into Category Table

INSERT INTO category
    (category_name, category_slug, category_description)
VALUES
    ('Clothing', 'clothing', 'Discover the latest trends in fashion with our Clothing category. From casual wear to elegant attire, find the perfect outfit for any occasion.'),
    ('Beauty & Personal Care', 'beauty-and-personal-care', 'Enhance your natural beauty and pamper yourself with our Beauty & Personal Care products. Explore a range of skincare, cosmetics, and grooming essentials.'),
    ('Shoes & Accessories', 'shoes-and-accessories', 'Step out in style with our Shoes & Accessories collection. Whether you''re looking for trendy footwear or statement accessories, we''ve got you covered.'),
    ('Toys & Games', 'toys-and-games', 'Spark imagination and endless fun with our Toys & Games selection. From educational toys to exciting games, there''s something for every age and interest.'),
    ('Arts & Crafts', 'arts-and-crafts', 'Unleash your creativity with our Arts & Crafts supplies. Dive into a world of colors, textures, and possibilities to bring your artistic visions to life.'),
    ('Electronics', 'electronics', 'Stay connected and up-to-date with our Electronics category. Discover the latest gadgets, devices, and tech innovations to elevate your digital lifestyle.');

--------------------------------- Create User Table

CREATE TABLE "user"
(
    user_id SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    mobile VARCHAR(50) UNIQUE ,
    password VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_admin BOOLEAN DEFAULT FALSE,
    is_banned BOOLEAN DEFAULT FALSE
);

---------------------------- Insert into User Table

INSERT INTO "user"
    (first_name, last_name, email, mobile, password, is_admin, is_banned)
VALUES
    ('Raghad', 'Alotaibi', 'Raghad@gmail.com', '0539482044', '11112', TRUE , FALSE),
    ('Somayah', 'Absi', 'somayah@gmail.com', '0556677343', '222221', FALSE , FALSE),
    ('Nada', 'Yhaya', 'Nada@gmail.com', '0539444478', '333311', FALSE , FALSE),
    ('Sadeem', 'Alghamdi', 'Sadeem@gmail.com', '0556678553', '15542', FALSE, FALSE),
    ('Albandri', 'Alotaibi', 'Albandri@gmail.com', '0556677223', '11442', FALSE, FALSE);

--------------------------------- Create Product Table

CREATE TABLE product
(
    product_id SERIAL PRIMARY KEY,
    product_name VARCHAR(50) NOT NULL,
    product_slug VARCHAR(100),
    product_description TEXT NOT NULL,
    product_price NUMERIC(10, 2) NOT NULL,
    product_image VARCHAR(255) DEFAULT '',
    product_quantity_in_stock INTEGER NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    category_id INTEGER,
    FOREIGN KEY (category_id) REFERENCES category(category_id)
);

---------------------------- Insert into Product Table
INSERT INTO product
    (product_name, product_slug, product_description, product_price, product_image, product_quantity_in_stock, category_id)
VALUES

    ('Perfume', 'perfume', 'Elegant fragrance for all occasions', 59.99, 'image', 50, 2),
    ('Sunscreen', 'sunscreen', 'Protect your skin from harmful UV rays', 100.98, 'image', 89, 2),
    ('Lipstick', 'lipstick', 'Add a pop of color to your lips with our creamy lipstick.', 25.55, 'image', 15, 2),
    ('Sunglasses', 'sunglasses', 'Stay stylish and protected from the sun with our fashionable sunglasses.', 45.75, 'image', 25, 3);

--------------------------------- Create Order Table


CREATE TABLE "order"
(
    order_id SERIAL PRIMARY KEY,
    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    order_status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    payment JSONB,
    user_id INTEGER,
    FOREIGN KEY (user_id) REFERENCES "user"(user_id)
);

---------------------------- Insert into Order Table


INSERT INTO "order"
    (order_date, order_status, payment, user_id)

VALUES
    ('2024-02-24', 'Processing', '{"method": "Credit Card" }', 3),
    ('2024-01-22', 'Closure', '{"method": "Credit Card" }', 2),
    ('2024-04-23', 'Canceled', '{"method": "Cash On Delivery" }', 4);


--------------------------------- Create Order Product Table


CREATE TABLE order_product
(
    order_product_id SERIAL PRIMARY KEY,
    quantity INT DEFAULT 1 CHECK (quantity >= 1),
    order_id INT,
    product_id INT,
    FOREIGN KEY (order_id) REFERENCES "order"(order_id),
    FOREIGN KEY (product_id) REFERENCES product(product_id)
);


---------------------------- Insert into Order Product Table


-- * Create a function to handle the insertion of order products just to increase quantity if id product exist
CREATE OR REPLACE FUNCTION insert_order_product_trigger
() 
RETURNS TRIGGER AS $$ 
BEGIN
    IF EXISTS (SELECT 1
    FROM order_product
    WHERE order_id = NEW.order_id AND product_id = NEW.product_id) THEN
    UPDATE order_product 
        SET quantity = quantity + NEW.quantity 
        WHERE order_id = NEW.order_id AND product_id = NEW.product_id;
    RETURN NULL;
    ELSE
    RETURN NEW;
END
IF; 
END; 
$$ LANGUAGE plpgsql;


-- * Create a trigger to invoke the trigger function before inserting into OrderProduct table 
CREATE TRIGGER before_insert_order_product_trigger 
BEFORE
INSERT ON
order_product
FOR
EACH
ROW
EXECUTE FUNCTION insert_order_product_trigger
();


-- * Insert to table
INSERT INTO order_product
    (quantity, order_id, product_id)
VALUES
    (2, 1, 3),
    (3, 1, 2),
    (1, 1, 2),
    (2, 1, 4);


-- * Function get_total_order_price()
CREATE OR REPLACE FUNCTION get_total_order_price
() RETURNS NUMERIC AS $$
DECLARE
    total_price NUMERIC := 0;
BEGIN
    SELECT
        SUM(order_total_price)
    INTO
        total_price
    FROM
        (
            SELECT
            COALESCE(SUM(p.product_price * op.quantity), 0) AS order_total_price
        FROM
            order_product op
            JOIN
            product p ON op.product_id = p.product_id
        GROUP BY
                op.order_id
        ) AS order_totals;

    RETURN total_price;
END;
$$ LANGUAGE plpgsql;


-- * Query with JOIN 
SELECT
    op.order_product_id,
    op.order_id,
    op.product_id,
    p.product_name,
    op.quantity,
    COALESCE(SUM(p.product_price * op.quantity), 0) AS total_price,
    get_total_order_price() AS total_order_price
FROM
    order_product op
    JOIN
    "order" oc ON op.order_id = oc.order_id
    JOIN
    product p ON op.product_id = p.product_id
WHERE
    op.order_id = 1
GROUP BY
    op.order_product_id,
    op.order_id,
    op.product_id,
    p.product_name,
    op.quantity;


-- Retrieve customer name, order ID, status, and date from orders table joined with users table.

SELECT CONCAT(c.first_name, ' ', c.last_name) AS customer_name,
    b.order_id,
    b.order_status,
    b.order_date
FROM  "order" b
INNER JOIN "user" c USING(user_id);

-- Retrieve customer name and last order date, sorted by order date in descending order.

SELECT  CONCAT(c.first_name, ' ', c.last_name) AS customer_name, b.order_date AS "last_orders"
FROM "order" b
INNER JOIN "user" c USING(user_id)
ORDER BY b.order_date DESC;

