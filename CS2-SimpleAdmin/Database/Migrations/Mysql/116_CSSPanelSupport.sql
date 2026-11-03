ALTER TABLE sa_servers
CHANGE COLUMN rcon_password rcon VARCHAR(255) NOT NULL;

ALTER TABLE `sa_admins`
ADD `servers_groups` int(11) NOT NULL AFTER `server_id`;

CREATE TABLE IF NOT EXISTS `sa_admins_groups` (
    `id` VARCHAR(50) NOT NULL,
    `name` TEXT NOT NULL,
    `flags` TEXT NOT NULL,
    `immunity` varchar(64) NOT NULL DEFAULT '0',
    `created` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `sa_servers_groups` (
    `id` int(11) NOT NULL AUTO_INCREMENT,
    `name` varchar(64) NOT NULL,
    `servers` varchar(64) NOT NULL,
    `created` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `sa_players_ips` (
    `id` int(11) NOT NULL AUTO_INCREMENT,
    `steamid` bigint(20) NOT NULL,
    `address` varchar(64) NOT NULL,
    `used_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`id`),
    UNIQUE KEY `steamid` (`steamid`, `address`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `sa_chatlogs` (
    `id` int(11) NOT NULL AUTO_INCREMENT,
    `serverId` varchar(64) NOT NULL,
    `playerSteam64` varchar(64) NOT NULL,
    `playerName` varchar(64) NOT NULL,
    `message` TEXT,
    `team` BOOLEAN NOT NULL,
    `created` timestamp NOT NULL,
    PRIMARY KEY (`id`)
) ENGINE = InnoDB AUTO_INCREMENT = 36 DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `sa_statistics` (
    `id` INT(11) NOT NULL AUTO_INCREMENT,
    `serverId` VARCHAR(64) NOT NULL COLLATE 'utf8mb4_general_ci',
    `playerId` VARCHAR(64) NOT NULL COLLATE 'utf8mb4_general_ci',
    `playerName` VARCHAR(64) NOT NULL COLLATE 'utf8mb4_general_ci',
    `playerIP` VARCHAR(64) NOT NULL COLLATE 'utf8mb4_general_ci',
    `connectDate` timestamp DEFAULT CURRENT_TIMESTAMP,
    `connectTime` INT(20) NOT NULL,
    `disconnectDate` timestamp NULL DEFAULT NULL,
    `disconnectTime` INT(20) NULL DEFAULT NULL,
    `duration` INT(20) NULL DEFAULT NULL,
    `kills` INT(10) NULL DEFAULT NULL,
    `flags` TEXT COLLATE 'utf8mb4_general_ci',
    `map` VARCHAR(64) NOT NULL COLLATE 'utf8mb4_general_ci',
    PRIMARY KEY (`id`) USING BTREE,
    INDEX `playerId` (`playerId`) USING BTREE
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;

-- Revert player_steamid columns back to VARCHAR so we can safely store non-numeric Steam IDs (e.g. bot/console values)
ALTER TABLE `sa_bans`
MODIFY `player_steamid` VARCHAR(64) NULL DEFAULT NULL;

ALTER TABLE `sa_mutes` MODIFY `player_steamid` VARCHAR(64) NOT NULL;

ALTER TABLE `sa_warns` MODIFY `player_steamid` VARCHAR(64) NOT NULL;

ALTER TABLE `sa_admins` MODIFY `player_steamid` VARCHAR(64) NOT NULL;
CREATE TABLE IF NOT EXISTS `sa_metrics` (
    `server_id` int(11) NOT NULL,
    `steamid` bigint(20) NOT NULL,
    `date` DATE NOT NULL,
    PRIMARY KEY (`server_id`, `steamid`, `date`),
    INDEX `server_date` (`server_id`, `date`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_general_ci;
