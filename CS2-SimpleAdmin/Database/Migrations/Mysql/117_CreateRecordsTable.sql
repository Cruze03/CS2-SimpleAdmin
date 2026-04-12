CREATE TABLE IF NOT EXISTS `sa_records` (
    `id`           INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `name`         VARCHAR(255) NOT NULL,
    `url`          VARCHAR(512) NULL,
    `reason`       VARCHAR(64)  NOT NULL DEFAULT 'manual',
    `steam_id`     VARCHAR(64)  NULL,
    `penalty_id`   INT          NULL,
    `penalty_type` TINYINT      NULL,
    `created`      BIGINT       NOT NULL,
    `server_id`    INT          NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
