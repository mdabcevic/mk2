-- Connect to the database
\c bartenderdb;

-- Enum types
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'subscriptiontier') THEN
    CREATE TYPE SubscriptionTier AS ENUM ('None', 'Trial', 'Basic', 'Standard', 'Premium', 'Enterprise');
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
    subscriptionTier SubscriptionTier NOT NULL DEFAULT 'None'
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
    seats INTEGER NOT NULL DEFAULT 2,
    status TableStatus NOT NULL DEFAULT 'empty',
    qrcode VARCHAR NULL
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
