CREATE DATABASE IF NOT EXISTS `chatbotdb`;
USE `chatbotdb`;

CREATE TABLE IF NOT EXISTS `chats` (
  `ChatID` int(11) NOT NULL AUTO_INCREMENT,
  `Username` varchar(50) DEFAULT NULL,
  `Message` text NOT NULL,
  `Response` text NOT NULL,
  `Timestamp` datetime DEFAULT current_timestamp(),
  PRIMARY KEY (`ChatID`),
  KEY `Username` (`Username`),
  CONSTRAINT `chats_ibfk_1` FOREIGN KEY (`Username`) REFERENCES `users` (`Username`)
) ENGINE=InnoDB AUTO_INCREMENT=54 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `users` (
  `Username` varchar(50) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `Password` varchar(50) NOT NULL,
  `RememberMe` tinyint(1) DEFAULT 0,
  `RecoveryCode` varchar(6) DEFAULT NULL,
  PRIMARY KEY (`Username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;