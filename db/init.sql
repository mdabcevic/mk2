-- Connect to the database
-- \c bartenderdb;

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
INSERT INTO businesses (o_i_b, name, headquarters, subscription_tier, created_at) VALUES
('12345678903', 'Vivas Bar', 'Neka Adresa 1', 'basic', now()),
('11115678903', 'Leggiero', 'Neka Adresa 2', 'basic', now()),
('22225678903', 'Bonaca', 'Selska cesta 28', 'none', now()),
('33335678903', 'Elixir', 'Selska cesta 28', 'basic', now()),
('12345678901', 'Sunset Bar', 'New York, NY', 'standard', now()),
('23456789012', 'Moonlight Lounge', 'Los Angeles, CA', 'trial', now()),
('34567890123', 'Cloud9 Café', 'Chicago, IL', 'premium', now());

-- Insert cities (used as foreign keys in Places)
INSERT INTO city (name, created_at) VALUES
('Zagreb', now()),
('Split', now()),
('Dubrovnik', now()),
('New York', now()),
('Los Angeles', now()),
('Chicago', now());

-- Insert places (city_id assumes order of Cities insert above)
INSERT INTO places (business_id, city_id, address, opens_at, closes_at, created_at, description) VALUES
(1, 1, 'Ilica 50', '07:00', '23:00', now(),'Kako smo smješteni na 20 lokacija od kojih 16 u Zagrebu, uvijek nas možeš posjetiti u blizini svojeg kvarta! Naš je imperativ tvoje zadovoljstvo te da se u svakom Vivasu osjećaš ugodno i opušteno kao u toplini svojeg doma! To postižemo kontinuiranim održavanjem postavljenog standarda, kvalitetnom uslugom kao i cijelim procesom izrade Vivas proizvoda.'),
(1, 1, 'Trg bana Jelačića 15', '08:00', '00:00', now(), 'Kako smo smješteni na 20 lokacija od kojih 16 u Zagrebu, uvijek nas možeš posjetiti u blizini svojeg kvarta! Naš je imperativ tvoje zadovoljstvo te da se u svakom Vivasu osjećaš ugodno i opušteno kao u toplini svojeg doma! To postižemo kontinuiranim održavanjem postavljenog standarda, kvalitetnom uslugom kao i cijelim procesom izrade Vivas proizvoda.'),
(2, 1, 'Radnička cesta 1', '06:30', '22:30', now(), 'Leggiero bar nalazi se na ulazu Slavonija gdje možete, uz ugodnu atmosferu, popiti vaše najdraže piće te se opustiti prije ili poslije dugog radnog dana. Leggiero ima i veliku terasu gdje možete probati osvježavajuća pića, Leggiero MIX svježe cijeđene sokove, a nakon 17 h počinje BEERanje pa svakodnevno potražite posebnu ponudu CRAFT i odabranih piva po posebnim cijenama. U blizini je i dječje igralište tako da roditelji mogu na miru popiti svoju omiljenu kavu, dok se klinci zabavljaju u blizini i pod budnim okom.'),
(2, 1, 'Jarunska 5', '07:00', '23:00', now(), 'U ugodnom ambijentu ovog lijepo uređenog kafića predahnite od kupovine uz jedan od toplih ili hladnih napitaka iz naše bogate ponude pića. Mjesto na kojem možeš pobjeći od svakodnevnog stresa. Mjesto gdje ćeš se u sasvim običnom danu osjećati kao na godišnjem!'),
(2, 2, 'Riva 2', '08:00', '23:00', now(), 'Želite popiti najfiniju Lavazza kavu i svježe cijeđeni sok, uživati u tek pečenom kroasanu i tostu ili se zasladiti najfinijim tortama u gradu? Sve to i još puno više pronaći ćete u Leggiero baru u prizemlju City Centera one West. Moderan i ugodan ambijent te uvijek ljubazno i uslužno osoblje, pobrinut će se da baš svaki trenutak proveden u Leggiero baru bude trenutak inspiracije i dobrog raspoloženja. Leggiero bar raspolaže i odvojenim prostorom koji je namijenjen pušačima. '),
(3, 2, 'Poljička cesta 35', '07:30', '22:30', now(), 'Bonaca – mjesto dobrog okusa i opuštene atmosfere. Bilo da dolaziš na jutarnju kavu, popodnevni predah ili večernje druženje, Bonaca ti nudi savršeno pripremljene napitke, ukusne kolače i ugodan ambijent. Naša terasa i toplo osoblje pobrinut će se da svaki tvoj dolazak bude pravo malo zadovoljstvo.'),
(3, 3, 'Obala Kneza Domagoja 10', '09:00', '23:30', now(), 'Bonaca – sad i na još jednoj lokaciji! Uz već poznatu toplinu i opušten ugođaj, Bonaca ti sada donosi svoje vrhunske kave, osvježavajuće napitke i fine slastice i na novoj lokaciji. Bilo da si na jutarnjem sastanku, usputnoj pauzi ili večernjem izlasku, Bonaca je uvijek pravi izbor za ugodan predah.'),
(4, 3, 'Stradun 25', '08:30', '00:00', now(), 'U Elixiru te čekaju pažljivo pripremljene kave, svježi sokovi, kokteli i fini zalogaji u modernom i ugodnom ambijentu. Bilo da dolaziš na kratku pauzu, jutarnji ritual ili večernje druženje, Elixir je mjesto gdje se energija puni, a dan postaje ljepši.'),
(5, 4, '5th Ave', '07:00', '21:00', now(), 'Sunset Bar je savršeno mjesto za bijeg od svakodnevice, gdje možeš uživati u pažljivo pripremljenoj kavi, osvježavajućim koktelima i ukusnim zalogajima u ugodnom i moderno uređenom prostoru, dok opuštajuća glazba i predivan pogled na zalazak sunca stvaraju jedinstvenu atmosferu za jutarnja druženja, popodnevne pauze ili večernje izlazke s prijateljima.'),
(6, 5, 'Sunset Blvd', '08:00', '22:00', now(), 'U elegantnom ambijentu, gdje se spoj modernog stila i opuštanja, možeš uživati u savršenoj kavi, vrhunskim koktelima i glazbi koja stvara posebnu atmosferu, bilo da tražiš miran kutak za opuštanje ili mjesto za večernje druženje. Sve to, uz nezaboravan doživljaj, pruža ti Moonlight Lounge, idealno mjesto za svaki trenutak.'),
(7, 6, 'Wacker Drive', '07:30', '23:00', now(), 'U opuštenoj atmosferi, gdje svaki detalj poziva na uživanje, možeš se opustiti uz savršeno pripremljenu kavu, svježe cijeđene sokove i ukusne grickalice, dok ti ugodna glazba i mirno okruženje pomažu da se osjećaš kao kod kuće. Sve ovo i još mnogo više nudi ti Cloud9 Cafe, mjesto koje je savršeno za uživanje u svakom trenutku dana.');

-- Insert staff (one per place)
INSERT INTO staff (place_id, o_i_b, username, password, full_name, role, created_at) VALUES
(1, '98765432101', 'vivasmanager', '$2a$12$nLebvsyCKkIcVDVmG3cdtO6Ag/6yIX55mVPsky7RBisYBX5/6y9pC', 'Petar Horvat', 'manager', now()), -- pw: test
(1, '98765432201', 'vivasilica_reg', '$2a$12$nLebvsyCKkIcVDVmG3cdtO6Ag/6yIX55mVPsky7RBisYBX5/6y9pC', 'M. D.', 'regular', now()), -- pw: test
(2, '98765432102', 'vivas_trg', '$2a$12$nxTG4512zsE.3g1n5A7Zaudg7gQsM4GNAq6DFEKKDcxWQNO/EbIsy', 'Maja Novak', 'regular', now()), --pw: authtest
(3, '98765432103', 'leggiero_radnicka', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Ivana Kovač', 'manager', now()),
(4, '98765432104', 'leggiero_jarun', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Marko Babić', 'regular', now()),
(5, '98765432105', 'bonaca_riva', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Ana Marić', 'manager', now()),
(6, '98765432106', 'bonaca_poljicka', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Luka Perić', 'regular', now()),
(7, '98765432107', 'elixir_obala', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Lucija Radić', 'manager', now()),
(8, '98765432108', 'elixir_stradun', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Nikola Jurić', 'regular', now()),
(9, '98765432109', 'sunset_admin', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Tom Smith', 'manager', now()),
(10, '98765432110', 'moonlight_admin', '$2a$12$AUQm5TA61U4mcd3Y3ql2M.avJzZ0625LMxZyeehn7y2FGA7o8vxIW', 'Samantha Lee', 'manager', now()), --pw: password
(11, '98765432111', 'cloud9_admin', '$2a$12$saDr9cjeFMH/hLcitmHg2O4xJK7Dtk5hqbb2q0Jm8mgTRatIcSVd2', 'James Chen', 'manager', now()); --pw: 123456

-- Our own business for testing superuser privileges 
INSERT INTO businesses (o_i_b, name, headquarters, subscription_tier, created_at) VALUES
('55555678901', 'Bartender Testing Owner Of Solution', 'Whatever Address Fits', 'premium', now());

-- Imaginary place under our test business
INSERT INTO places (business_id, city_id, address, opens_at, closes_at, created_at)
VALUES (8, 1, 'Whatever Address Fits', '08:00', '22:00', now());

-- Insert 4 employees with different privileges
-- owner
INSERT INTO staff (place_id, o_i_b, username, password, full_name, role, created_at)
VALUES (12, '99999999901', 'testowner', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Ivan Vlasnić', 'owner', now()); -- pw: test

-- administrator
INSERT INTO staff (place_id, o_i_b, username, password, full_name, role, created_at)
VALUES (12, '99999999902', 'testadmin', '$2a$12$fM2YwoCUJ/rm3jtaPc7dwuC/x252uZfzows4m3EAi9fDTyGpt/XJu', 'Ana Adminić', 'admin', now()); -- pw: test

-- manager
INSERT INTO staff (place_id, o_i_b, username, password, full_name, role, created_at)
VALUES (12, '99999999903', 'testmanager', '$2a$12$J2dfx2x4Iwqb6Xgbnm5XQurW196fEGal9LvEmrC5wR8M4DFKsPKry', 'Marko Menadžer', 'manager', now()); -- pw: test123

-- waiter (regular employee, manages table and marks orders as complete)
INSERT INTO staff (place_id, o_i_b, username, password, full_name, role, created_at)
VALUES (12, '99999999904', 'teststaff', '$2a$12$gZqmOoeAos6cXBVMSeTHge6YSTExR34fyfPcJXmi8WZw3L5Ea1Il6', 'Petra Konobarić', 'regular', now()); -- pw: 123456

-- admin for Vivas (place_id = 1)
INSERT INTO staff (place_id, o_i_b, username, password, full_name, role, created_at)
VALUES (1, '99999999905', 'vivasadmin', '$2a$12$iF54En7VicKnz3G6eBosf.m5HezRaQ6c2CyZCB.MUowFMzJayK9Dq', 'Luka Vivasović', 'admin', now()); -- pw: test123

INSERT INTO tables (id, place_id, label, seats, width, height, x, y, status, qr_salt, is_disabled, created_at) VALUES
(1, 1, '1', 2, 80, 80, 100.0, 100.0, 'empty', '5036144c6f5d41aeb0e332ea0029e073', false, now()),
(2, 1, '2', 2, 80, 80, 200.0, 100.0, 'empty', 'f8b4d726faf1436089415d0e453d33a3', false, now()),
(3, 1, '3', 2, 80, 80, 300.0, 100.0, 'empty', '766f575f7bf042ccb79e9df9da4e9ca5', true, now()),
(4, 1, '4', 4, 100, 100, 100.0, 200.0, 'empty', '768e63c7ab2b44a482b2a825645aaabb', false, now()),
(5, 1, '5', 4, 100, 100, 200.0, 200.0, 'empty', '1b3593e63a6a4fef8f2e5eae19840165', false, now()),
(6, 1, '6', 4, 100, 100, 300.0, 200.0, 'empty', 'ef9bf913754048b083a8571b740fb112', false, now()),
(7, 1, '7', 4, 100, 100, 400.0, 200.0, 'empty', '52206960508e41a797f546dd4106cf45', false, now()),
(8, 3, '1', 4, 90, 90, 100.0, 300.0, 'empty', 'e6fae97a5c54471984572d1020388970', false, now()),
(9, 3, '2', 4, 90, 90, 200.0, 300.0, 'empty', 'eb754108919e4db18cb0d05e2c4262f2', false, now());


-- This tells Postgres to set the sequence to the current max value
SELECT setval('tables_id_seq', (SELECT MAX(id) FROM tables));

-- Insert ProductCategory
INSERT INTO product_category (name, parent_category_id, created_at) VALUES
('Kave', null, now()),
('Bezalkoholna pića', null, now()),
('Topli napitci', null, now()),
('Alkohol', null, now()),
('Gazirana pića', 3, now()),
('Dodaci', null, now()),
('Vode', 3, now()),
('Pivo', 5, now()),
('Žestoka pića', 5, now()),
('Kokteli', 5, now()),
('Vino', 5, now()),
('Cider', 12, now()),
('Hrana', null, now()),
('Deserti', 14, now()),
('Specijalna ponuda', null, now()),
('Ostalo', null, now());

-- Insert products
INSERT INTO products (name, volume, category_id, created_at) VALUES
('Espresso', 'ŠAL', 1, now()),
('Kava s Mlijekom S', 'ŠAL', 1, now()),
('Kava s Mlijekom L', 'ŠAL', 1, now()),
('Cappucino', 'ŠAL', 1, now()),
('Bijela Kava', 'ŠAL', 1, now()),
('Kava sa Šlagom S', 'ŠAL', 1, now()),
('Kava sa Šlagom L', 'ŠAL', 1, now()),
('Kava sa Zobenim Mlijekom S', 'ŠAL', 1, now()),
('Kava sa Zobenim Mlijekom L', 'ŠAL', 1, now()),
('Bijela Kava sa Zobenim Mlijekom', 'ŠAL', 1, now()),
('Matcha Latte', 'ŠAL', 1, now()),
('Espresso Bez Kofeina', 'ŠAL', 1, now()),
('Kava Bez Kofeina S', 'ŠAL', 1, now()),
('Kava Bez Kofeina L', 'ŠAL', 1, now()),
('Cappuccino Bez Kofeina L', 'ŠAL', 1, now()),
('Bijela Kava Bez Kofeina', 'ŠAL', 1, now()),


('Kakao', 'ŠAL', 3, now()),
('Nescafe Classic', 'ŠAL', 3, now()),
('Nescafe Vanilija', 'ŠAL', 3, now()),
('Nescafe Čokolada', 'ŠAL', 3, now()),
('Nescafe Irish', 'ŠAL', 3, now()),
('Topla Čokolada Tamna', 'ŠAL', 3, now()),
('Topla Čokolada Bijela', 'ŠAL', 3, now()),
('Topla Čokolada Tamna sa Šlagom', 'ŠAL', 3, now()),
('Topla Čokolada Bijela sa Šlagom', 'ŠAL', 3, now()),
('Čaj s Limunom i Medom - Zeleni', 'ŠAL', 3, now()),
('Čaj s Limunom i Medom - Zeleni s Okusom', 'ŠAL', 3, now()),
('Čaj s Limunom i Medom - Šumsko Voće', 'ŠAL', 3, now()),
('Čaj s Limunom i Medom - Crni', 'ŠAL', 3, now()),
('Čaj s Limunom i Medom - Šipak', 'ŠAL', 3, now()),
('Čaj s Limunom i Medom - Menta', 'ŠAL', 3, now()),
('Čaj s Limunom i Medom - Kamilica', 'ŠAL', 3, now()),
('Čaj s Limunom i Medom - Jabuka Aronija', 'ŠAL', 3, now()),
('Čaj s Limunom i Medom - Naranča Cimet', 'ŠAL', 3, now()),
('Čaj s Limunom i Medom - Đumbir Limun', 'ŠAL', 3, now()),
('Čaj s Limunom i Medom - Jabuka Cimet', 'ŠAL', 3, now()),

('Coca-Cola', '0.25L', 2, now()),
('Coca-Cola Zero', '0.25L', 2, now()),
('Coca-Cola Zero Sugar Zero Caffeine', '0.25L', 2, now()),
('Fanta', '0.25L', 2, now()),
('Sprite', '0.25L', 2, now()),
('Schweppes Tangerine', '0.25L', 2, now()),
('Schweppes Bitter Lemon', '0.25L', 2, now()),
('Schweppes Pink Grapefruit', '0.25L', 2, now()),
('Schweppes Tonic', '0.25L', 2, now()),
('Schweppes Botanical Tonic Zero', '0.20L', 2, now()),
('Three Cents Tonic', '0.20L', 2, now()),
('Three Cents Pink Grapefruit', '0.20L', 2, now()),
('Cockta', '0.275L', 2, now()),
('Cockta Free', '0.275L', 2, now()),
('Cedevita Limun', '0.25L', 2, now()),
('Cedevita Naranča', '0.25L', 2, now()),
('Cedevita Bazga & Limun', '0.25L', 2, now()),
('Cedevita Limeta', '0.25L', 2, now()),
('Cedevita Ananas & Mango', '0.25L', 2, now()),
('Cedevita Grejp', '0.25L', 2, now()),
('Pago Ananas', '0.20L', 2, now()),
('Pago Crni Ribizl', '0.20L', 2, now()),
('Pago Jabuka', '0.20L', 2, now()),
('Pago Jagoda', '0.20L', 2, now()),
('Pago Marelica', '0.20L', 2, now()),
('Pago Naranča', '0.20L', 2, now()),
('Pipi Naranča', '0.25L', 2, now()),
('Jana Ledeni Čaj Breskva', '0.33L', 2, now()),
('Jana Ledeni Čaj Brusnica', '0.33L', 2, now()),
('Jana Ledeni Čaj Limun', '0.33L', 2, now()),
('Orangina', '0.25L', 2, now()),
('Red Bull', '0.25L', 2, now()),
('Hydra Iso', '0.50L', 2, now()),

('Cookies&Cream: Lješnjak', 'KOM', 14, now()),
('Cookies&Cream: Pistacija', 'KOM', 14, now()),
('Brownie sa sladoledom', 'KOM', 14, now()),
('Bueno Cake', 'KOM', 14, now()),
('Cheesecake Classic', 'KOM', 14, now()),
('Cheesecake Sezonski okusi', 'KOM', 14, now()),
('Ferrero Cake', 'KOM', 14, now()),
('Snikers Cake', 'KOM', 14, now()),
('Sladoled od vanilije', 'KOM', 14, now()),

('Royal Fresh Sendvič', 'KOM', 13, now()),
('Focaccia Sendvič', 'KOM', 13, now()),
('Ciabatta Sendvič', 'KOM', 13, now()),
('Tost Šunka Sir', 'KOM', 13, now()),

('Jana', '0.33L', 7, now()),
('Jamnica', '0.33L', 7, now()),
('Jamnica Limunada', '0.33L', 7, now()),
('Jamnica Narančada', '0.33L', 7, now()),
('Jamnica Sensation Bazga-limun', '0.25L', 7, now()),
('Jamnica Sensation Limeta-kiwano', '0.25L', 7, now()),
('Jamnica Sensation Limunska Trava', '0.25L', 7, now()),
('Jana Vitamin Immuno Limun', '0.33L', 7, now()),
('Jana Vitamin Happy Naranča', '0.33L', 7, now()),
('Jana Vitamin Refresh', '0.33L', 7, now()),
('Romerquelle Emotion Bazga-marelica', '0.33L', 7, now()),

('Beck''s', '0.33L', 8, now()),
('Beck''s', '0.50L', 8, now()),
('Corona', '0.355L', 8, now()),
('Leffe Blonde', '0.33L', 8, now()),
('Nikšićko', '0.50L', 8, now()),
('Ožujsko', '0.33L', 8, now()),
('Ožujsko', '0.50L', 8, now()),
('Ožujsko Cool', '0.50L', 8, now()),
('Staropramen', '0.33L', 8, now()),
('Staropramen', '0.50L', 8, now()),
('Stella Artois', '0.33L', 8, now()),
('Vukovarsko', '0.50L', 8, now()),
('Tomislav', '0.50L', 8, now()),
('Leffe Brown', '0.33L', 8, now()),
('Staropramen', '0.30L', 8, now()),
('Staropramen', '0.50L', 8, now()),
('Grif New England Pale Ale', '0.30L', 8, now()),
('Grif New England Pale Ale', '0.50L', 8, now()),
('Ožujsko Limun', '0.50L', 8, now()),
('Ožujsko Grejp', '0.50L', 8, now()),

('Jack Daniels', '0.03', 9, now()),
('Bombay Sapphire', '0.03', 9, now()),
('Tanqueray', '0.03', 9, now()),
('Liker Medica', '0.03', 9, now()),
('Liker Višnja', '0.03', 9, now()),
('Liker Borovnica', '0.03', 9, now()),
('Liker Suha Šljiva', '0.03', 9, now()),
('Šljivovica', '0.03', 9, now()),
('Travarica', '0.03', 9, now()),

('Sauvignon Blanc Apolitico', '0.10L', 11, now()),
('Malvazija Menghetti', '0.10L', 11, now()),
('Graševina Apolitico', '0.75L', 11, now()),

('Somersby Jabuka', '0.33', 12, now()),
('Somersby Kruška', '0.33', 12, now()),
('Somersby Borovnica', '0.33', 12, now()),
('Somersby Marakuja Naranča', '0.33', 12, now()),
('Somersby Mango Limeta', '0.33', 12, now()),
('Somersby Lubenica', '0.33', 12, now()),

('Aperol Spritz', 'KOM', 10, now()),
('Hugo', 'KOM', 10, now()),
('Cuba Libre', 'KOM', 10, now()),
('Classic Mai Tai', 'KOM', 10, now()),
('Mojito', 'KOM', 10, now());

-- Insert Menu
INSERT INTO menu_items (place_id, product_id, price, is_available, created_at) VALUES
--kave
(1, 1, 2.20, true, now()),
(1, 2, 2.30, true, now()),
(1, 3, 2.40, true, now()),
(1, 4, 2.40, true, now()),
(1, 5, 2.80, true, now()),
(1, 6, 2.30, true, now()),
(1, 7, 2.40, true, now()),
(1, 8, 2.40, true, now()),
(1, 9, 2.50, true, now()),
(1, 10, 2.90, true, now()),
(1, 11, 3.90, true, now()),
(1, 12, 2.30, true, now()),
(1, 13, 2.40, true, now()),
(1, 14, 2.50, true, now()),
(1, 15, 2.60, true, now()),
(1, 16, 2.80, true, now()),
(1, 17, 2.40, true, now()),
(1, 18, 2.50, true, now()),

--topli napitci
(1, 19, 2.80, true, now()),
(1, 20, 2.90, true, now()),
(1, 21, 2.90, true, now()),
(1, 22, 2.90, true, now()),
(1, 23, 2.90, true, now()),
(1, 24, 3.10, true, now()),
(1, 25, 3.10, true, now()),
(1, 26, 3.30, true, now()),
(1, 27, 3.30, true, now()),
(1, 28, 2.50, true, now()),
(1, 29, 2.50, true, now()),
(1, 30, 2.50, true, now()),
(1, 31, 2.50, true, now()),
(1, 32, 2.50, true, now()),
(1, 33, 2.50, true, now()),
(1, 34, 2.50, true, now()),
(1, 35, 2.50, true, now()),
(1, 36, 2.50, true, now()),
(1, 37, 2.50, true, now()),
(1, 38, 2.50, true, now()),

-- bezalkoholna pića
(1, 39, 3.30, true, now()),
(1, 40, 3.30, true, now()),
(1, 41, 3.30, true, now()),
(1, 42, 3.30, true, now()),
(1, 43, 3.30, true, now()),
(1, 44, 3.30, true, now()),
(1, 45, 3.30, true, now()),
(1, 46, 3.30, true, now()),
(1, 47, 3.30, true, now()),
(1, 48, 3.30, true, now()),
(1, 49, 3.30, true, now()),
(1, 50, 3.30, true, now()),
(1, 51, 3.30, true, now()),
(1, 52, 3.30, true, now()),

--vode
(1, 53, 2.70, true, now()),
(1, 54, 2.70, true, now()),
(1, 55, 2.70, true, now()),
(1, 56, 2.70, true, now()),
(1, 57, 2.70, true, now()),
(1, 58, 2.70, true, now()),
(1, 59, 3.50, true, now()),
(1, 60, 3.50, true, now()),
(1, 61, 3.50, true, now()),
(1, 62, 3.50, true, now()),
(1, 63, 3.50, true, now()),
(1, 64, 3.50, true, now()),
(1, 65, 3.30, true, now()),
(1, 66, 3.30, true, now()),
(1, 67, 3.30, true, now()),
(1, 68, 3.30, true, now()),
(1, 69, 3.30, true, now()),
(1, 70, 3.50, true, now()),
(1, 71, 3.80, true, now()),

--ostalo
(1, 72, 2.20, true, now()),
(1, 73, 2.40, true, now()),
(1, 85, 2.70, true, now()),
(1, 86, 2.70, true, now()),
(1, 87, 3.30, true, now()),
(1, 88, 3.30, true, now()),
(1, 92, 2.90, true, now()),
(1, 93, 2.90, true, now()),
(1, 94, 2.90, true, now()),
(1, 98, 4.20, true, now()),
(1, 101, 3.00, true, now()),
(1, 102, 3.30, true, now()),
(1, 103, 3.30, true, now()),
(1, 104, 3.00, true, now()),
(1, 105, 3.30, true, now()),
(1, 119, 2.40, true, now()),
(1, 120, 2.40, true, now()),
(1, 121, 2.40, true, now()),
(1, 123, 2.40, true, now()),
(1, 128, 3.40, true, now()),
(1, 129, 3.40, true, now()),
(1, 130, 3.40, true, now());

-- second bar
INSERT INTO menu_items (place_id, product_id, price, is_available, created_at) VALUES
(3, 3, 2.30, true, now()),
(3, 5, 2.60, true, now()),
(3, 7, 2.30, true, now()),
(3, 4, 2.30, true, now()),
(3, 19, 2.70, true, now()),
(3, 20, 2.80, true, now()),
(3, 21, 2.80, true, now()),
(3, 22, 2.80, true, now()),
(3, 23, 2.80, true, now()),
(3, 92, 2.90, true, now()),
(3, 93, 2.90, true, now()),
(3, 94, 2.90, true, now()),
(3, 76, 3.90, true, now());

INSERT INTO orders (table_id, created_at, status, total_price, payment_type) VALUES
(1, (NOW() - interval '1 day'), 'closed', 14.50, 'cash'),
(2, (NOW() - interval '2 day'), 'closed', 10.50, 'cash'),
(8, (NOW() - interval '1 hour'), 'closed', 13.00, 'cash'),
(8, (NOW() - interval '1 day'), 'closed', 11.75, 'cash');

INSERT INTO products_per_orders (order_id, menu_item_id, price, discount, count) VALUES
(1, 31, 2.50, 0, 1),
(1, 4, 2.40, 0, 2),
(2, 58, 2.70, 0, 1),
(2, 71, 3.80, 0, 2),
(2, 85, 3.00, 0, 1),
(3, 98, 2.60, 0, 2),
(3, 106, 2.90, 0, 1),
(4, 105, 2.80, 0, 2),
(4, 103, 2.80, 0, 1);

INSERT INTO place_pictures (place_id, url, image_type, is_visible, created_at) VALUES
(1, 'https://vivasbar.hr/slide3_00000.jpg', 'banner', true, now()),
(1, 'https://www.streetsofzagreb.com/wp-content/uploads/2018/10/vivas-bundek-1024x945.jpg', 'gallery', true, now()),
(1, 'https://www.shopping-centar-precko.com/EasyEdit/UserFiles/ShopImages/vivas-bar/vivas-bar-634662193013957578-1_720_540.jpeg', 'gallery', true, now()),
(1, 'https://vivasbar.hr/Coffee.jpg', 'gallery', true, now()),
(1, 'https://www.cityparkzelina.hr/wp-content/uploads/2022/06/Vivasbar_totem_vizual_1.png', 'logo', true, now()),
(2, 'https://tc-jarun.com/wp-content/uploads/2021/03/K-69.jpg', 'banner', true, now()),
(2, 'https://www.cityparkzelina.hr/wp-content/uploads/2022/06/Vivasbar_totem_vizual_1.png', 'logo', true, now()),
(3, 'https://leggiero.hr/wp-content/uploads/2022/03/Family-mall-2048x1365.jpg', 'banner', true, now()),
(3, 'https://leggiero.hr/wp-content/uploads/2024/03/Leggiero-3062-scaled.jpg', 'gallery', true, now()),
(3, 'https://leggiero.hr/wp-content/uploads/2024/04/cropped-cropped-leggiero-logo-2019-bijeli-01-1024x341.png', 'logo', true, now()),
(3, 'https://leggiero.hr/wp-content/uploads/2024/03/Leggiero-08818.jpg', 'logo', true, now()),
(4, 'https://leggiero.hr/wp-content/uploads/2024/04/Dizajn-bez-naslova-88-768x576.png', 'banner', true, now()),
(5, 'https://portanova.hr/uploads/images/store/image/21/073edfeb-94ec-43d2-b560-f4ecb9f8b82e.jpg', 'banner', true, now()),
(8, 'https://lh5.googleusercontent.com/p/AF1QipN_ljbkFuvWmX_GQLmH8ua0VOPn3KRAjM0gec1i=w650-h365-k-no', 'banner', true, now()),
(9, 'https://greatlocations.com/wp-content/uploads/2023/07/sunset-club-1.jpg', 'banner', true, now()),
(10, 'https://media-cdn.tripadvisor.com/media/photo-s/13/8a/25/b6/lighting-up-the-night.jpg', 'banner', true, now()),
(11, 'https://dynamic-media-cdn.tripadvisor.com/media/photo-o/19/63/5d/31/cloud9-cafe-bar-at-our.jpg?w=1000&h=-1&s=1', 'banner', true, now())