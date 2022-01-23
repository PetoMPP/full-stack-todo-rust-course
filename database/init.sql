CREATE TABLE IF NOT EXISTS users (
  id          SERIAL PRIMARY KEY,
  username    VARCHAR(64) NOT NULL UNIQUE,
  password    VARCHAR(64) NOT NULL,
  deleted_at  TIMESTAMP DEFAULT NULL,
  token       TEXT UNIQUE DEFAULT NULL
);

CREATE TABLE IF NOT EXISTS tasks (
  id            SERIAL PRIMARY KEY,
  priority      VARCHAR(4) DEFAULT NULL,
  title         VARCHAR(255) NOT NULL,
  completed_at  TIMESTAMP DEFAULT NULL,
  description   TEXT DEFAULT NULL,
  deleted_at    TIMESTAMP DEFAULT NULL,
  user_id       INTEGER NOT NULL, 
  is_default    BOOLEAN DEFAULT FALSE,
  CONSTRAINT fk_users FOREIGN KEY (user_id) REFERENCES users(id)
);

INSERT INTO users (username, password) VALUES ('deleteduser', '$2b$12$x3hs5oMgjHdcV1GUEElfsO19JtS6.ixJAX9Cj62GyhpdPAIW25sky');

INSERT INTO tasks (title, deleted_at, user_id) VALUES (
  'my deleted task',
  NOW(),
  (select id from users where username = 'deleteduser')
);