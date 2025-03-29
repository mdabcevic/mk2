-- Connect to the database
\c bartenderdb;

-- Enum types
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'subscriptiontier') THEN
    CREATE TYPE SubscriptionTier AS ENUM ('none', 'trial', 'basic', 'standard', 'premium', 'enterprise');
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'employeerole') THEN
    CREATE TYPE EmployeeRole AS ENUM ('admin', 'manager', 'regular');
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'tablestatus') THEN
    CREATE TYPE TableStatus AS ENUM ('empty', 'occupied', 'reserved');
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'productcategory') THEN
    CREATE TYPE ProductCategory AS ENUM ('softDrinks', 'hotBeverages', 'alcohol', 'desert', 'sparklingDrinks', 'other');
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'orderstatus') THEN
    CREATE TYPE OrderStatus AS ENUM ('created', 'approved', 'delivered', 'paid', 'closed', 'cancelled');
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'paymenttype') THEN
    CREATE TYPE PaymentType AS ENUM ('cash', 'creditcard', 'other');
  END IF;
END$$;

-- Table: Business
CREATE TABLE IF NOT EXISTS Businesses (
    id SERIAL PRIMARY KEY,
    OIB VARCHAR NOT NULL UNIQUE,
    name VARCHAR NOT NULL,
    headquarters VARCHAR,
    subscriptionTier SubscriptionTier NOT NULL DEFAULT 'none'
);

-- Table: Cities
CREATE TABLE IF NOT EXISTS Cities (
    id SERIAL PRIMARY KEY,
    name VARCHAR NOT NULL
);

-- Table: Places
CREATE TABLE IF NOT EXISTS Places (
    id SERIAL PRIMARY KEY,
    business_id INTEGER NOT NULL REFERENCES Businesses(id) ON DELETE CASCADE,
    city_id INTEGER NOT NULL REFERENCES Cities(id) ON DELETE CASCADE,
    address VARCHAR NOT NULL,
    opensAt TIME DEFAULT now(),
    closesAt TIME DEFAULT now()
);

-- Table: Staff
CREATE TABLE IF NOT EXISTS Staff (
    id SERIAL PRIMARY KEY,
    place_id INTEGER NOT NULL REFERENCES Places(id) ON DELETE CASCADE,
    OIB VARCHAR NOT NULL UNIQUE,
    username VARCHAR NOT NULL,
    password VARCHAR NOT NULL,
    FullName VARCHAR NOT NULL,
    role EmployeeRole NOT NULL DEFAULT 'regular'
);

-- Table: Tables
CREATE TABLE IF NOT EXISTS Tables (
    id SERIAL PRIMARY KEY,
    place_id INTEGER NOT NULL REFERENCES Places(id) ON DELETE CASCADE,
    label VARCHAR not null,
    seats INTEGER NOT NULL DEFAULT 2,
    status TableStatus NOT NULL DEFAULT 'empty',
    qrsalt text NOT NULL,
    isdisabled boolean DEFAULT false

);

-- Table: Products
CREATE TABLE IF NOT EXISTS Products (
    id SERIAL PRIMARY KEY,
    name VARCHAR NOT NULL,
    category ProductCategory
);

-- Table: MenuItems
CREATE TABLE IF NOT EXISTS MenuItems (
    place_id INTEGER NOT NULL REFERENCES Places(id) ON DELETE CASCADE,
    product_id INTEGER NOT NULL REFERENCES Products(id) ON DELETE CASCADE,
    price DECIMAL NOT NULL DEFAULT 0.0,
    isAvailable BOOLEAN NOT NULL,
    description VARCHAR NULL,
    PRIMARY KEY (place_id, product_id)
);

-- Table: Customers
CREATE TABLE IF NOT EXISTS Customers (
    id SERIAL PRIMARY KEY,
    username VARCHAR NOT NULL,
    password VARCHAR NOT NULL
);

-- Table: Orders
CREATE TABLE IF NOT EXISTS Orders (
    id SERIAL PRIMARY KEY,
    table_id INTEGER NOT NULL UNIQUE REFERENCES Tables(id) ON DELETE CASCADE,
    customer_id INTEGER REFERENCES Customers(id) ON DELETE SET NULL,
    createdAt TIMESTAMP DEFAULT now(),
    status OrderStatus NOT NULL DEFAULT 'created',
    paymentType PaymentType NOT NULL DEFAULT 'cash'
);

-- Table: ProductsPerOrder
CREATE TABLE IF NOT EXISTS ProductsPerOrder (
    order_id INTEGER NOT NULL REFERENCES Orders(id) ON DELETE CASCADE,
    product_id INTEGER NOT NULL REFERENCES Products(id) ON DELETE CASCADE,
    count INTEGER DEFAULT 1,
    PRIMARY KEY (order_id, product_id)
);

-- Table: Reviews
CREATE TABLE IF NOT EXISTS Reviews (
    product_id INTEGER NOT NULL REFERENCES Products(id) ON DELETE CASCADE,
    customer_id INTEGER NOT NULL REFERENCES Customers(id) ON DELETE CASCADE,
    rating INTEGER NOT NULL,
    comment VARCHAR NULL,
    PRIMARY KEY (product_id, customer_id)
);

-- Insert subscription-tiered businesses with enum values
INSERT INTO Businesses (OIB, name, headquarters, subscriptionTier) VALUES
('12345678903', 'Vivas Bar', 'Neka Adresa 1', 'basic'),
('11115678903', 'Leggiero', 'Neka Adresa 2', 'basic'),
('22225678903', 'Bonaca', 'Selska cesta 28', 'none'),
('33335678903', 'Elixir', 'Selska cesta 28', 'basic'),
('12345678901', 'Sunset Bar', 'New York, NY', 'standard'),
('23456789012', 'Moonlight Lounge', 'Los Angeles, CA', 'trial'),
('34567890123', 'Cloud9 Café', 'Chicago, IL', 'premium');

-- Insert cities (used as foreign keys in Places)
INSERT INTO Cities (name) VALUES
('Zagreb'),
('Split'),
('Dubrovnik'),
('New York'),
('Los Angeles'),
('Chicago');

-- Insert places (city_id assumes order of Cities insert above)
INSERT INTO Places (business_id, city_id, address, opensAt, closesAt) VALUES
(1, 1, 'Ilica 50', '07:00', '23:00'),
(1, 1, 'Trg bana Jelačića 15', '08:00', '00:00'),
(2, 1, 'Radnička cesta 1', '06:30', '22:30'),
(2, 1, 'Jarunska 5', '07:00', '23:00'),
(2, 2, 'Riva 2', '08:00', '23:00'),
(3, 2, 'Poljička cesta 35', '07:30', '22:30'),
(3, 3, 'Obala Kneza Domagoja 10', '09:00', '23:30'),
(4, 3, 'Stradun 25', '08:30', '00:00'),
(5, 4, '5th Ave', '07:00', '21:00'),
(6, 5, 'Sunset Blvd', '08:00', '22:00'),
(7, 6, 'Wacker Drive', '07:30', '23:00');

-- Insert staff (one per place)
INSERT INTO Staff (place_id, OIB, username, password, fullName, role) VALUES
(1, '98765432101', 'vivas_ilica', 'hashed_password', 'Petar Horvat', 'manager'),
(2, '98765432102', 'vivas_trg', 'hashed_password', 'Maja Novak', 'regular'),
(3, '98765432103', 'leggiero_radnicka', 'hashed_password', 'Ivana Kovač', 'manager'),
(4, '98765432104', 'leggiero_jarun', 'hashed_password', 'Marko Babić', 'regular'),
(5, '98765432105', 'bonaca_riva', 'hashed_password', 'Ana Marić', 'manager'),
(6, '98765432106', 'bonaca_poljicka', 'hashed_password', 'Luka Perić', 'regular'),
(7, '98765432107', 'elixir_obala', 'hashed_password', 'Lucija Radić', 'manager'),
(8, '98765432108', 'elixir_stradun', 'hashed_password', 'Nikola Jurić', 'regular'),
(9, '98765432109', 'sunset_admin', 'hashed_password', 'Tom Smith', 'manager'),
(10, '98765432110', 'moonlight_admin', 'hashed_password', 'Samantha Lee', 'manager'),
(11, '98765432111', 'cloud9_admin', 'hashed_password', 'James Chen', 'manager');