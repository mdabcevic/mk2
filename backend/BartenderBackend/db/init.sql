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
    seats INTEGER NOT NULL DEFAULT 2,
    status TableStatus NOT NULL DEFAULT 'empty',
    qrcode VARCHAR NULL
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
    place_id INTEGER NOT NULL REFERENCES Places(id) ON DELETE CASCADE,
    product_id INTEGER NOT NULL REFERENCES Products(id) ON DELETE CASCADE,
    price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
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
('Espresso', 'ŠAL', 2),
('Kava s Mlijekom Mala', 'ŠAL', 2),
('Kava s Mlijekom Velika', 'ŠAL', 2),
('Cappucino', 'ŠAL', 2),
('Bijela Kava', 'ŠAL', 2),
('Kava sa Šlagom Mala', 'ŠAL', 2),
('Kava sa Šlagom Velika', 'ŠAL', 2),
('Kava sa Zobenim Mlijekom Mala', 'ŠAL', 2),
('Kava sa Zobenim Mlijekom Velika', 'ŠAL', 2),
('Bijela Kava sa Zobenim Mlijekom', 'ŠAL', 2),
('Matcha Latte', 'ŠAL', 2),
('Espresso Bez Kofeina', 'ŠAL', 2),
('Kava Bez Kofeina s Mlijekom Mala', 'ŠAL', 2),
('Kava Bez Kofeina s Mlijekom Velika', 'ŠAL', 2),
('Cappuccino Bez Kofeina s Mlijekom Velika', 'ŠAL', 2),
('Bijela Kava Bez Kofeina', 'ŠAL', 2),
('Kava sa Zobenim Mlijekom Bez Kofeina Mala', 'ŠAL', 2),
('Kava sa Zobenim Mlijekom Bez Kofeina Velika', 'ŠAL', 2),

('Kakao', 'ŠAL', 4),
('Nescafe Classic', 'ŠAL', 4),
('Nescafe Vanilija', 'ŠAL', 4),
('Nescafe Čokolada', 'ŠAL', 4),
('Nescafe Irish', 'ŠAL', 4),
('Topla Čokolada Tamna', 'ŠAL', 4),
('Topla Čokolada Bijela', 'ŠAL', 4),
('Topla Čokolada Tamna sa Šlagom', 'ŠAL', 4),
('Topla Čokolada Bijela sa Šlagom', 'ŠAL', 4),
('Čaj s Limunom i Medom - Zeleni', 'ŠAL', 4),
('Čaj s Limunom i Medom - Zeleni s Okusom', 'ŠAL', 4),
('Čaj s Limunom i Medom - Šumsko Voće', 'ŠAL', 4),
('Čaj s Limunom i Medom - Crni', 'ŠAL', 4),
('Čaj s Limunom i Medom - Šipak', 'ŠAL', 4),
('Čaj s Limunom i Medom - Menta', 'ŠAL', 4),
('Čaj s Limunom i Medom - Kamilica', 'ŠAL', 4),
('Čaj s Limunom i Medom - Jabuka Aronija', 'ŠAL', 4),
('Čaj s Limunom i Medom - Naranča Cimet', 'ŠAL', 4),
('Čaj s Limunom i Medom - Đumbir Limun', 'ŠAL', 4),
('Čaj s Limunom i Medom - Jabuka Cimet', 'ŠAL', 4),

('Coca-Cola', '0.25L', 3),
('Coca-Cola Zero', '0.25L', 3),
('Coca-Cola Zero Sugar Zero Caffeine', '0.25L', 3),
('Fanta', '0.25L', 3),
('Sprite', '0.25L', 3),
('Schweppes Tangerine', '0.25L', 3),
('Schweppes Bitter Lemon', '0.25L', 3),
('Schweppes Pink Grapefruit', '0.25L', 3),
('Schweppes Tonic', '0.25L', 3),
('Schweppes Botanical Tonic Zero', '0.20L', 3),
('Three Cents Tonic', '0.20L', 3),
('Three Cents Pink Grapefruit', '0.20L', 3),
('Cockta', '0.275L', 3),
('Cockta Free', '0.275L', 3),
('Cedevita Limun', '0.25L', 3),
('Cedevita Naranča', '0.25L', 3),
('Cedevita Bazga & Limun', '0.25L', 3),
('Cedevita Limeta', '0.25L', 3),
('Cedevita Ananas & Mango', '0.25L', 3),
('Cedevita Grejp', '0.25L', 3),
('Pago Ananas', '0.20L', 3),
('Pago Crni Ribizl', '0.20L', 3),
('Pago Jabuka', '0.20L', 3),
('Pago Jagoda', '0.20L', 3),
('Pago Marelica', '0.20L', 3),
('Pago Naranča', '0.20L', 3),
('Pipi Naranča', '0.25L', 3),
('Jana Ledeni Čaj Breskva', '0.33L', 3),
('Jana Ledeni Čaj Brusnica', '0.33L', 3),
('Jana Ledeni Čaj Limun', '0.33L', 3),
('Orangina', '0.25L', 3),
('Red Bull', '0.25L', 3),
('Hydra Iso', '0.50L', 3),


('Cookies&Cream: Lješnjak', 'KOM', 15),
('Cookies&Cream: Pistacija', 'KOM', 15),
('Brownie sa sladoledom', 'KOM', 15),
('Bueno Cake', 'KOM', 15),
('Cheesecake Classic', 'KOM', 15),
('Cheesecake Sezonski okusi', 'KOM', 15),
('Ferrero Cake', 'KOM', 15),
('Snikers Cake', 'KOM', 15),
('Sladoled od vanilije', 'KOM', 15),

('Royal Fresh Sendvič', 'KOM', 14),
('Focaccia Sendvič', 'KOM', 14),
('Ciabatta Sendvič', 'KOM', 14),
('Tost Šunka Sir', 'KOM', 14),

('Jana', '0.33L', 8),
('Jamnica', '0.33L', 8),
('Jamnica Limunada', '0.33L', 8),
('Jamnica Narančada', '0.33L', 8),
('Jamnica Sensation Bazga-limun', '0.25L', 8),
('Jamnica Sensation Limeta-kiwano', '0.25L', 8),
('Jamnica Sensation Limunska Trava', '0.25L', 8),
('Jana Vitamin Immuno Limun', '0.33L', 8),
('Jana Vitamin Happy Naranča', '0.33L', 8),
('Jana Vitamin Refresh', '0.33L', 8),
('Romerquelle Emotion Bazga-marelica', '0.33L', 8),

('Beck''s', '0.33L', 9),
('Beck''s', '0.50L', 9),
('Corona', '0.355L', 9),
('Leffe Blonde', '0.33L', 9),
('Nikšićko', '0.50L', 9),
('Ožujsko', '0.33L', 9),
('Ožujsko', '0.50L', 9),
('Ožujsko Cool', '0.50L', 9),
('Staropramen', '0.33L', 9),
('Staropramen', '0.50L', 9),
('Stella Artois', '0.33L', 9),
('Vukovarsko', '0.50L', 9),
('Tomislav', '0.50L', 9),
('Leffe Brown', '0.33L', 9),
('Staropramen', '0.30L', 9),
('Staropramen', '0.50L', 9),
('Grif New England Pale Ale', '0.30L', 9),
('Grif New England Pale Ale', '0.50L', 9),
('Ožujsko Limun', '0.50L', 9),
('Ožujsko Grejp', '0.50L', 9),

('Jack Daniels', '0.03', 10),
('Bombay Sapphire', '0.03', 10),
('Tanqueray', '0.03', 10),
('Liker Medica', '0.03', 10),
('Liker Višnja', '0.03', 10),
('Liker Borovnica', '0.03', 10),
('Liker Suha Šljiva', '0.03', 10),
('Šljivovica', '0.03', 10),
('Travarica', '0.03', 10),

('Sauvignon Blanc Apolitico', '0.10L', 12),
('Malvazija Menghetti', '0.10L', 12),
('Graševina Apolitico', '0.75L', 12),

('Somersby Jabuka', '0.33', 13),
('Somersby Kruška', '0.33', 13),
('Somersby Borovnica', '0.33', 13),
('Somersby Marakuja Naranča', '0.33', 13),
('Somersby Mango Limeta', '0.33', 13),
('Somersby Lubenica', '0.33', 13),

('Aperol Spritz', 'KOM',11),
('Hugo', 'KOM',11),
('Cuba Libre', 'KOM',11),
('Classic Mai Tai', 'KOM',11),
('Mojito', 'KOM',11);

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
--kave
(3, 3, 2.30, true),
(3, 5, 2.60, true),
(3, 7, 2.30, true),
(3, 4, 2.30, true),

--topli napitci
(3, 19, 2.70, true),
(3, 20, 2.80, true),
(3, 21, 2.80, true),
(3, 22, 2.80, true),
(3, 23, 2.80, true),

(3, 92, 2.90, true),
(3, 93, 2.90, true),
(3, 94, 2.90, true),

(3, 76, 3.90, true);