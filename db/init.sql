-- Connect to the database
\c bartenderdb;

-- Enum types
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'subscriptiontier') THEN
    CREATE TYPE SubscriptionTier AS ENUM ('none', 'trial', 'basic', 'standard', 'premium', 'enterprise');
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'employeerole') THEN
    CREATE TYPE EmployeeRole AS ENUM ('owner', 'admin', 'manager', 'regular');
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'tablestatus') THEN
    CREATE TYPE TableStatus AS ENUM ('empty', 'occupied', 'reserved');
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'orderstatus') THEN
    CREATE TYPE OrderStatus AS ENUM ('created', 'approved', 'delivered', 'payment_requested', 'paid', 'closed', 'cancelled');
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
    width INTEGER NOT NULL,
    height INTEGER NOT NULL,
    xcoordinate DECIMAL(6,2) NOT NULL,
    ycoordinate DECIMAL(6,2) NOT NULL,
    status TableStatus NOT NULL DEFAULT 'empty',
    qrsalt text NOT NULL,
    isdisabled boolean DEFAULT false,
    UNIQUE (place_id, label) -- ensures table tags are unique per place
);

-- Create session group table
CREATE TABLE GuestSessionGroups (
    id UUID PRIMARY KEY,
    table_id INT NOT NULL REFERENCES Tables(id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT now(),
    passphrase VARCHAR(12) NOT NULL
);

-- Table: GuestSessions
CREATE TABLE IF NOT EXISTS guestSessions (
    id UUID PRIMARY KEY,
    table_id INTEGER NOT NULL REFERENCES tables(id) ON DELETE CASCADE,
    group_id UUID REFERENCES GuestSessionGroups(id) ON DELETE SET NULL,
    token TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMP NOT NULL,
    isvalid boolean default true
);

-- Table: ProductCategory
CREATE TABLE IF NOT EXISTS ProductCategory (
    id SERIAL PRIMARY KEY,
    name VARCHAR NOT NULL,
	parentcategory_id INTEGER NULL REFERENCES ProductCategory(id) ON DELETE SET NULL
);

-- Table: Products
CREATE TABLE IF NOT EXISTS Products (
    id SERIAL PRIMARY KEY,
    name VARCHAR NOT NULL,
	volume VARCHAR NULL,
    category_id INTEGER NOT NULL REFERENCES ProductCategory(id),
	business_id INTEGER REFERENCES Businesses(id) ON DELETE CASCADE
);

-- Table: MenuItems
CREATE TABLE IF NOT EXISTS MenuItems (
	id SERIAL PRIMARY KEY,
    place_id INTEGER NOT NULL REFERENCES Places(id) ON DELETE CASCADE,
    product_id INTEGER NOT NULL REFERENCES Products(id) ON DELETE CASCADE,
    price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    isAvailable BOOLEAN NOT NULL DEFAULT false,
    description VARCHAR NULL,
    UNIQUE(place_id, product_id)
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
    table_id INTEGER NOT NULL REFERENCES Tables(id) ON DELETE CASCADE,
    customer_id INTEGER REFERENCES Customers(id) ON DELETE SET NULL,
	guest_session_id UUID REFERENCES guestSessions(id) ON DELETE SET NULL,
    createdAt TIMESTAMP DEFAULT now(),
    status OrderStatus NOT NULL DEFAULT 'created',
	total_price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    paymentType PaymentType NOT NULL DEFAULT 'cash',
	note VARCHAR NULL
);

-- Table: ProductsPerOrder
CREATE TABLE IF NOT EXISTS ProductsPerOrder (
    order_id INTEGER NOT NULL REFERENCES Orders(id) ON DELETE CASCADE,
    menuitem_id INTEGER NOT NULL REFERENCES MenuItems(id) ON DELETE CASCADE,
	item_price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
	discount DECIMAL(5,2) DEFAULT 0.00,
    count INTEGER DEFAULT 1,
    PRIMARY KEY (order_id, menuitem_id)
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
(1, '98765432101', 'vivasmanager', '$2a$12$nLebvsyCKkIcVDVmG3cdtO6Ag/6yIX55mVPsky7RBisYBX5/6y9pC', 'Petar Horvat', 'manager'), -- pw: test
(1, '98765432201', 'vivasilica_reg', '$2a$12$nLebvsyCKkIcVDVmG3cdtO6Ag/6yIX55mVPsky7RBisYBX5/6y9pC', 'M. D.', 'regular'), -- pw: test
(2, '98765432102', 'vivas_trg', '$2a$12$nxTG4512zsE.3g1n5A7Zaudg7gQsM4GNAq6DFEKKDcxWQNO/EbIsy', 'Maja Novak', 'regular'), --pw: authtest
(3, '98765432103', 'leggiero_radnicka', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Ivana Kovač', 'manager'),
(4, '98765432104', 'leggiero_jarun', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Marko Babić', 'regular'),
(5, '98765432105', 'bonaca_riva', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Ana Marić', 'manager'),
(6, '98765432106', 'bonaca_poljicka', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Luka Perić', 'regular'),
(7, '98765432107', 'elixir_obala', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Lucija Radić', 'manager'),
(8, '98765432108', 'elixir_stradun', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Nikola Jurić', 'regular'),
(9, '98765432109', 'sunset_admin', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Tom Smith', 'manager'),
(10, '98765432110', 'moonlight_admin', '$2a$12$AUQm5TA61U4mcd3Y3ql2M.avJzZ0625LMxZyeehn7y2FGA7o8vxIW', 'Samantha Lee', 'manager'), --pw: password
(11, '98765432111', 'cloud9_admin', '$2a$12$saDr9cjeFMH/hLcitmHg2O4xJK7Dtk5hqbb2q0Jm8mgTRatIcSVd2', 'James Chen', 'manager'); --pw: 123456

-- Our own business for testing superuser privileges 
INSERT INTO Businesses (OIB, name, headquarters, subscriptionTier) VALUES
('55555678901', 'Bartender Testing Owner Of Solution', 'Whatever Address Fits', 'premium');

-- Imaginary place under our test business
INSERT INTO Places (business_id, city_id, address, opensAt, closesAt)
VALUES (8, 1, 'Whatever Address Fits', '08:00', '22:00');

-- Insert 4 employees with different privileges
-- owner
INSERT INTO Staff (place_id, OIB, username, password, fullName, role)
VALUES (12, '99999999901', 'testowner', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Ivan Vlasnić', 'owner'); -- pw: test

-- administrator
INSERT INTO Staff (place_id, OIB, username, password, fullName, role)
VALUES (12, '99999999902', 'testadmin', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Ana Adminić', 'admin'); -- pw: test

-- manager
INSERT INTO Staff (place_id, OIB, username, password, fullName, role)
VALUES (12, '99999999903', 'testmanager', '$2a$12$J2dfx2x4Iwqb6Xgbnm5XQurW196fEGal9LvEmrC5wR8M4DFKsPKry', 'Marko Menadžer', 'manager'); -- pw: test123

-- waiter (regular employee, manages table and marks orders as complete)
INSERT INTO Staff (place_id, OIB, username, password, fullName, role)
VALUES (12, '99999999904', 'teststaff', '$2a$12$gZqmOoeAos6cXBVMSeTHge6YSTExR34fyfPcJXmi8WZw3L5Ea1Il6', 'Petra Konobarić', 'regular'); -- pw: 123456

-- admin for Vivas (place_id = 1)
INSERT INTO Staff (place_id, OIB, username, password, fullName, role)
VALUES (1, '99999999905', 'vivasadmin', '$2a$12$iF54En7VicKnz3G6eBosf.m5HezRaQ6c2CyZCB.MUowFMzJayK9Dq', 'Luka Vivasović', 'admin'); -- pw: test123

INSERT INTO Tables (id, place_id, label, seats, width, height, xcoordinate, ycoordinate, status, qrsalt, isdisabled) VALUES
(1, 1, '1', 2, 80, 80, 100.0, 100.0, 'empty', '5036144c6f5d41aeb0e332ea0029e073', false),
(2, 1, '2', 2, 80, 80, 200.0, 100.0, 'empty', 'f8b4d726faf1436089415d0e453d33a3', false),
(3, 1, '3', 2, 80, 80, 300.0, 100.0, 'empty', '766f575f7bf042ccb79e9df9da4e9ca5', true),
(4, 1, '4', 4, 100, 100, 100.0, 200.0, 'empty', '768e63c7ab2b44a482b2a825645aaabb', false),
(5, 1, '5', 4, 100, 100, 200.0, 200.0, 'empty', '1b3593e63a6a4fef8f2e5eae19840165', false),
(6, 1, '6', 4, 100, 100, 300.0, 200.0, 'empty', 'ef9bf913754048b083a8571b740fb112', false),
(7, 1, '7', 4, 100, 100, 400.0, 200.0, 'empty', '52206960508e41a797f546dd4106cf45', false),
(8, 3, '1', 4, 90, 90, 100.0, 300.0, 'empty', 'e6fae97a5c54471984572d1020388970', false),
(9, 3, '2', 4, 90, 90, 200.0, 300.0, 'empty', 'eb754108919e4db18cb0d05e2c4262f2', false);

-- This tells Postgres to set the sequence to the current max value
SELECT setval('tables_id_seq', (SELECT MAX(id) FROM tables));

-- Insert ProductCategory
INSERT INTO ProductCategory(name, parentcategory_id) VALUES
('Kave', null),
('Bezalkoholna pića', null),
('Topli napitci', null),
('Alkohol', null),
('Gazirana pića', 3),
('Dodaci', null),
('Vode', 3),
('Pivo', 5),
('Žestoka pića', 5),
('Kokteli', 5),
('Vino', 5),
('Cider', 12),
('Hrana', null),
('Deserti', 14),
('Specijalna ponuda', null),
('Ostalo', null);

-- Insert products
INSERT INTO Products(name, volume, category_id) VALUES
('Espresso', 'ŠAL', 1),
('Kava s Mlijekom S', 'ŠAL', 1),
('Kava s Mlijekom L', 'ŠAL', 1),
('Cappucino', 'ŠAL', 1),
('Bijela Kava', 'ŠAL', 1),
('Kava sa Šlagom S', 'ŠAL', 1),
('Kava sa Šlagom L', 'ŠAL', 1),
('Kava sa Zobenim Mlijekom S', 'ŠAL', 1),
('Kava sa Zobenim Mlijekom L', 'ŠAL', 1),
('Bijela Kava sa Zobenim Mlijekom', 'ŠAL', 1),
('Matcha Latte', 'ŠAL', 1),
('Espresso Bez Kofeina', 'ŠAL', 1),
('Kava Bez Kofeina S', 'ŠAL', 1),
('Kava Bez Kofeina L', 'ŠAL', 1),
('Cappuccino Bez Kofeina L', 'ŠAL', 1),
('Bijela Kava Bez Kofeina', 'ŠAL', 1),


('Kakao', 'ŠAL', 3),
('Nescafe Classic', 'ŠAL', 3),
('Nescafe Vanilija', 'ŠAL', 3),
('Nescafe Čokolada', 'ŠAL', 3),
('Nescafe Irish', 'ŠAL', 3),
('Topla Čokolada Tamna', 'ŠAL', 3),
('Topla Čokolada Bijela', 'ŠAL', 3),
('Topla Čokolada Tamna sa Šlagom', 'ŠAL', 3),
('Topla Čokolada Bijela sa Šlagom', 'ŠAL', 3),
('Čaj s Limunom i Medom - Zeleni', 'ŠAL', 3),
('Čaj s Limunom i Medom - Zeleni s Okusom', 'ŠAL', 3),
('Čaj s Limunom i Medom - Šumsko Voće', 'ŠAL', 3),
('Čaj s Limunom i Medom - Crni', 'ŠAL', 3),
('Čaj s Limunom i Medom - Šipak', 'ŠAL', 3),
('Čaj s Limunom i Medom - Menta', 'ŠAL', 3),
('Čaj s Limunom i Medom - Kamilica', 'ŠAL', 3),
('Čaj s Limunom i Medom - Jabuka Aronija', 'ŠAL', 3),
('Čaj s Limunom i Medom - Naranča Cimet', 'ŠAL', 3),
('Čaj s Limunom i Medom - Đumbir Limun', 'ŠAL', 3),
('Čaj s Limunom i Medom - Jabuka Cimet', 'ŠAL', 3),

('Coca-Cola', '0.25L', 2),
('Coca-Cola Zero', '0.25L', 2),
('Coca-Cola Zero Sugar Zero Caffeine', '0.25L', 2),
('Fanta', '0.25L', 2),
('Sprite', '0.25L', 2),
('Schweppes Tangerine', '0.25L', 2),
('Schweppes Bitter Lemon', '0.25L', 2),
('Schweppes Pink Grapefruit', '0.25L', 2),
('Schweppes Tonic', '0.25L', 2),
('Schweppes Botanical Tonic Zero', '0.20L', 2),
('Three Cents Tonic', '0.20L', 2),
('Three Cents Pink Grapefruit', '0.20L', 2),
('Cockta', '0.275L', 2),
('Cockta Free', '0.275L', 2),
('Cedevita Limun', '0.25L', 2),
('Cedevita Naranča', '0.25L', 2),
('Cedevita Bazga & Limun', '0.25L', 2),
('Cedevita Limeta', '0.25L', 2),
('Cedevita Ananas & Mango', '0.25L', 2),
('Cedevita Grejp', '0.25L', 2),
('Pago Ananas', '0.20L', 2),
('Pago Crni Ribizl', '0.20L', 2),
('Pago Jabuka', '0.20L', 2),
('Pago Jagoda', '0.20L', 2),
('Pago Marelica', '0.20L', 2),
('Pago Naranča', '0.20L', 2),
('Pipi Naranča', '0.25L', 2),
('Jana Ledeni Čaj Breskva', '0.33L', 2),
('Jana Ledeni Čaj Brusnica', '0.33L', 2),
('Jana Ledeni Čaj Limun', '0.33L', 2),
('Orangina', '0.25L', 2),
('Red Bull', '0.25L', 2),
('Hydra Iso', '0.50L', 2),


('Cookies&Cream: Lješnjak', 'KOM', 14),
('Cookies&Cream: Pistacija', 'KOM', 14),
('Brownie sa sladoledom', 'KOM', 14),
('Bueno Cake', 'KOM', 14),
('Cheesecake Classic', 'KOM', 14),
('Cheesecake Sezonski okusi', 'KOM', 14),
('Ferrero Cake', 'KOM', 14),
('Snikers Cake', 'KOM', 14),
('Sladoled od vanilije', 'KOM', 14),

('Royal Fresh Sendvič', 'KOM', 13),
('Focaccia Sendvič', 'KOM', 13),
('Ciabatta Sendvič', 'KOM', 13),
('Tost Šunka Sir', 'KOM', 13),

('Jana', '0.33L', 7),
('Jamnica', '0.33L', 7),
('Jamnica Limunada', '0.33L', 7),
('Jamnica Narančada', '0.33L', 7),
('Jamnica Sensation Bazga-limun', '0.25L', 7),
('Jamnica Sensation Limeta-kiwano', '0.25L', 7),
('Jamnica Sensation Limunska Trava', '0.25L', 7),
('Jana Vitamin Immuno Limun', '0.33L', 7),
('Jana Vitamin Happy Naranča', '0.33L', 7),
('Jana Vitamin Refresh', '0.33L', 7),
('Romerquelle Emotion Bazga-marelica', '0.33L', 7),

('Beck''s', '0.33L', 8),
('Beck''s', '0.50L', 8),
('Corona', '0.355L', 8),
('Leffe Blonde', '0.33L', 8),
('Nikšićko', '0.50L', 8),
('Ožujsko', '0.33L', 8),
('Ožujsko', '0.50L', 8),
('Ožujsko Cool', '0.50L', 8),
('Staropramen', '0.33L', 8),
('Staropramen', '0.50L', 8),
('Stella Artois', '0.33L', 8),
('Vukovarsko', '0.50L', 8),
('Tomislav', '0.50L', 8),
('Leffe Brown', '0.33L', 8),
('Staropramen', '0.30L', 8),
('Staropramen', '0.50L', 8),
('Grif New England Pale Ale', '0.30L', 8),
('Grif New England Pale Ale', '0.50L', 8),
('Ožujsko Limun', '0.50L', 8),
('Ožujsko Grejp', '0.50L', 8),

('Jack Daniels', '0.03', 9),
('Bombay Sapphire', '0.03', 9),
('Tanqueray', '0.03', 9),
('Liker Medica', '0.03', 9),
('Liker Višnja', '0.03', 9),
('Liker Borovnica', '0.03', 9),
('Liker Suha Šljiva', '0.03', 9),
('Šljivovica', '0.03', 9),
('Travarica', '0.03', 9),

('Sauvignon Blanc Apolitico', '0.10L', 11),
('Malvazija Menghetti', '0.10L', 11),
('Graševina Apolitico', '0.75L', 11),

('Somersby Jabuka', '0.33', 12),
('Somersby Kruška', '0.33', 12),
('Somersby Borovnica', '0.33', 12),
('Somersby Marakuja Naranča', '0.33', 12),
('Somersby Mango Limeta', '0.33', 12),
('Somersby Lubenica', '0.33', 12),

('Aperol Spritz', 'KOM',10),
('Hugo', 'KOM',10),
('Cuba Libre', 'KOM',10),
('Classic Mai Tai', 'KOM',10),
('Mojito', 'KOM',10);

-- Insert Menu
INSERT INTO MenuItems(place_id, product_id, price, isAvailable) VALUES
--kave
(1, 1, 2.20, true),
(1, 2, 2.30, true),
(1, 3, 2.40, true),
(1, 4, 2.40, true),
(1, 5, 2.80, true),
(1, 6, 2.30, true),
(1, 7, 2.40, true),
(1, 8, 2.40, true),
(1, 9, 2.50, true),
(1, 10, 2.90, true),
(1, 11, 3.90, true),
(1, 12, 2.30, true),
(1, 13, 2.40, true),
(1, 14, 2.50, true),
(1, 15, 2.60, true),
(1, 16, 2.80, true),
(1, 17, 2.40, true),
(1, 18, 2.50, true),

--topli napitci
(1, 19, 2.80, true),
(1, 20, 2.90, true),
(1, 21, 2.90, true),
(1, 22, 2.90, true),
(1, 23, 2.90, true),
(1, 24, 3.10, true),
(1, 25, 3.10, true),
(1, 26, 3.30, true),
(1, 27, 3.30, true),
(1, 28, 2.50, true),
(1, 29, 2.50, true),
(1, 30, 2.50, true),
(1, 31, 2.50, true),
(1, 32, 2.50, true),
(1, 33, 2.50, true),
(1, 34, 2.50, true),
(1, 35, 2.50, true),
(1, 36, 2.50, true),
(1, 37, 2.50, true),
(1, 38, 2.50, true),

-- bezalkoholna pića
(1, 39, 3.30, true),
(1, 40, 3.30, true),
(1, 41, 3.30, true),
(1, 42, 3.30, true),
(1, 43, 3.30, true),
(1, 44, 3.30, true),
(1, 45, 3.30, true),
(1, 46, 3.30, true),
(1, 47, 3.30, true),
(1, 48, 3.30, true),
(1, 49, 3.30, true),
(1, 50, 3.30, true),
(1, 51, 3.30, true),
(1, 52, 3.30, true),

--vode
(1, 53, 2.70, true),
(1, 54, 2.70, true),
(1, 55, 2.70, true),
(1, 56, 2.70, true),
(1, 57, 2.70, true),
(1, 58, 2.70, true),
(1, 59, 3.50, true),
(1, 60, 3.50, true),
(1, 61, 3.50, true),
(1, 62, 3.50, true),
(1, 63, 3.50, true),
(1, 64, 3.50, true),
(1, 65, 3.30, true),
(1, 66, 3.30, true),
(1, 67, 3.30, true),
(1, 68, 3.30, true),
(1, 69, 3.30, true),
(1, 70, 3.50, true),
(1, 71, 3.80, true),

--ostalo
(1, 72, 2.20, true),
(1, 73, 2.40, true),
(1, 85, 2.70, true),
(1, 86, 2.70, true),
(1, 87, 3.30, true),
(1, 88, 3.30, true),
(1, 92, 2.90, true),
(1, 93, 2.90, true),
(1, 94, 2.90, true),
(1, 98, 4.20, true),
(1, 101, 3.00, true),
(1, 102, 3.30, true),
(1, 103, 3.30, true),
(1, 104, 3.00, true),
(1, 105, 3.30, true),
(1, 119, 2.40, true),
(1, 120, 2.40, true),
(1, 121, 2.40, true),
(1, 123, 2.40, true),
(1, 128, 3.40, true),
(1, 129, 3.40, true),
(1, 130, 3.40, true),
(1, 134, 4.90, true),
(1, 137, 5.00, true),
(1, 138, 5.20, true);

-- second bar
INSERT INTO MenuItems(place_id, product_id, price, isAvailable) VALUES
(3, 3, 2.30, true),
(3, 5, 2.60, true),
(3, 7, 2.30, true),
(3, 4, 2.30, true),
(3, 19, 2.70, true),
(3, 20, 2.80, true),
(3, 21, 2.80, true),
(3, 22, 2.80, true),
(3, 23, 2.80, true),
(3, 92, 2.90, true),
(3, 93, 2.90, true),
(3, 94, 2.90, true),
(3, 76, 3.90, true);

INSERT INTO Orders(table_id, createdAt, status, total_price, paymentType) VALUES
(1, (NOW() - interval '1 day'), 'closed', 14.50, 'cash'),
(2, (NOW() - interval '2 day'), 'closed', 10.50, 'cash'),
(8, (NOW() - interval '1 hour'), 'closed', 13.00, 'cash'),
(8, (NOW() - interval '1 day'), 'closed', 11.75, 'cash');

INSERT INTO ProductsPerOrder(order_id, menuitem_id, item_price, discount, count) VALUES
(1, 31, 2.50, 0, 1),
(1, 4, 2.40, 0, 2),
(2, 58, 2.70, 0, 1),
(2, 71, 3.80, 0, 2),
(2, 85, 3.00, 0, 1),
(3, 98, 2.60, 0, 2),
(3, 106, 2.90, 0, 1),
(4, 105, 2.80, 0, 2),
(4, 103, 2.80, 0, 1);