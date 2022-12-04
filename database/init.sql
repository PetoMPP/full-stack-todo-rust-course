CREATE TABLE IF NOT EXISTS users (
  id                     SERIAL PRIMARY KEY,
  username               VARCHAR(64) NOT NULL UNIQUE,
  password               VARCHAR(128) NOT NULL,
  normalized_username    VARCHAR(64) NOT NULL UNIQUE,
  access                 INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS tasks (
  id            SERIAL PRIMARY KEY,
  priority      VARCHAR(4) DEFAULT NULL,
  title         VARCHAR(255) NOT NULL,
  completed_at  TIMESTAMP DEFAULT NULL,
  created_at    TIMESTAMP NOT NULL DEFAULT current_timestamp,
  description   TEXT DEFAULT NULL,
  user_id       INTEGER NOT NULL,
  CONSTRAINT fk_users FOREIGN KEY (user_id) REFERENCES users(id)
);
