CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    username VARCHAR(100) NOT NULL,
    email VARCHAR(100),
    address TEXT,
    phone VARCHAR(100),
    website VARCHAR(100),
    company TEXT
);