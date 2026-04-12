CREATE TABLE IF NOT EXISTS `sa_records` (
    `id`           INTEGER PRIMARY KEY AUTOINCREMENT,
    `name`         TEXT    NOT NULL,
    `url`          TEXT    NULL,
    `reason`       TEXT    NOT NULL DEFAULT 'manual',
    `steam_id`     TEXT    NULL,
    `penalty_id`   INTEGER NULL,
    `penalty_type` INTEGER NULL,
    `created`      INTEGER NOT NULL,
    `server_id`    INTEGER NULL
);
