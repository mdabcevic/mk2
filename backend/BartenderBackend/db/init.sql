-- Ensure the database exists (optional, since it's created via POSTGRES_DB)
-- CREATE DATABASE bartenderdb;

-- Connect to the database
\c bartenderdb;

-- Table: Business
CREATE TABLE Business (
    id SERIAL PRIMARY KEY,
    OIB VARCHAR NOT NULL UNIQUE,
    name VARCHAR NOT NULL,
    headquarters VARCHAR,
    subscription_tier INTEGER DEFAULT 1
);

-- Table: Places
CREATE TABLE Places (
    id SERIAL PRIMARY KEY,
    business_id INTEGER NOT NULL REFERENCES Business(id) ON DELETE CASCADE,
    location VARCHAR UNIQUE,
    opensAt TIME DEFAULT now(),
    closesAt TIME DEFAULT now()
);

-- Table: Staff
CREATE TABLE Staff (
    id SERIAL PRIMARY KEY,
    place_id INTEGER NOT NULL REFERENCES Places(id) ON DELETE CASCADE,
    OIB VARCHAR NOT NULL UNIQUE,
    username VARCHAR NOT NULL,
    password VARCHAR NOT NULL,
    FullName VARCHAR NOT NULL,
    role VARCHAR NOT NULL DEFAULT 'bartender'
);

-- Table: Tables
CREATE TABLE Tables (
    id SERIAL PRIMARY KEY,
    place_id INTEGER NOT NULL REFERENCES Places(id) ON DELETE CASCADE,
    seats INTEGER NOT NULL DEFAULT 2,
    status VARCHAR NOT NULL DEFAULT 'empty',
    qrcode VARCHAR NULL
);

-- Table: Products
CREATE TABLE Products (
    id SERIAL PRIMARY KEY,
    name VARCHAR NOT NULL
);

-- Table: MenuItems
CREATE TABLE MenuItems (
    place_id INTEGER NOT NULL REFERENCES Places(id) ON DELETE CASCADE,
    product_id INTEGER NOT NULL REFERENCES Products(id) ON DELETE CASCADE,
    price DECIMAL NOT NULL DEFAULT 0.0,
    quantity VARCHAR NOT NULL,
    description VARCHAR NULL,
    PRIMARY KEY (place_id, product_id)
);

-- Table: Customers
CREATE TABLE Customers (
    id SERIAL PRIMARY KEY,
    username VARCHAR NOT NULL,
    password VARCHAR NOT NULL
);

-- Table: Orders
CREATE TABLE Orders (
    id SERIAL PRIMARY KEY,
    table_id INTEGER NOT NULL UNIQUE REFERENCES Tables(id) ON DELETE CASCADE,
    customer_id INTEGER REFERENCES Customers(id) ON DELETE SET NULL,
    created_at TIMESTAMP DEFAULT now(),
    status VARCHAR NOT NULL DEFAULT 'created',
    payment_type VARCHAR NOT NULL DEFAULT 'cash'
);

-- Table: ProductsPerOrder
CREATE TABLE ProductsPerOrder (
    order_id INTEGER NOT NULL REFERENCES Orders(id) ON DELETE CASCADE,
    product_id INTEGER NOT NULL REFERENCES Products(id) ON DELETE CASCADE,
    count INTEGER DEFAULT 1,
    PRIMARY KEY (order_id, product_id)
);

-- Table: Reviews
CREATE TABLE Reviews (
    product_id INTEGER NOT NULL REFERENCES Products(id) ON DELETE CASCADE,
    customer_id INTEGER NOT NULL REFERENCES Customers(id) ON DELETE CASCADE,
    rating INTEGER NOT NULL,
    comment VARCHAR NULL,
    PRIMARY KEY (product_id, customer_id)
);
