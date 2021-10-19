/*
MySQL Backup
Database: Trace
Backup Time: 2021-10-18 16:44:40
*/

SET FOREIGN_KEY_CHECKS=0;
DROP TABLE IF EXISTS `Trace`.`clickconfig`;
DROP TABLE IF EXISTS `Trace`.`website`;
CREATE TABLE `clickconfig` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `SiteId` int(11) NOT NULL,
  `Name` varchar(50) DEFAULT NULL COMMENT '名称',
  `UrlRule` varchar(255) DEFAULT NULL COMMENT '应用页面',
  `AttrNames` varchar(2000) DEFAULT NULL COMMENT '元素选择器',
  `PeekConfig` varchar(500) DEFAULT NULL COMMENT '需要记录的元素属性值',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
CREATE TABLE `website` (
  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT '网站信息表',
  `Name` varchar(50) DEFAULT NULL COMMENT '网站名称',
  `SiteDomain` varchar(255) DEFAULT NULL COMMENT '网站域名',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
BEGIN;
LOCK TABLES `Trace`.`clickconfig` WRITE;
DELETE FROM `Trace`.`clickconfig`;
UNLOCK TABLES;
COMMIT;
BEGIN;
LOCK TABLES `Trace`.`website` WRITE;
DELETE FROM `Trace`.`website`;
UNLOCK TABLES;
COMMIT;
