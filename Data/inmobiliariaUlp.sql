-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: inmobiliariaulp
-- ------------------------------------------------------
-- Server version	8.0.41

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `contratos`
--

DROP TABLE IF EXISTS `contratos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `contratos` (
  `id_contrato` int NOT NULL AUTO_INCREMENT,
  `id_inmueble` int NOT NULL,
  `id_inquilino` int NOT NULL,
  `id_usuario` int NOT NULL,
  `id_usuario_finaliza` int DEFAULT NULL,
  `fecha_inicio` date NOT NULL,
  `fecha_fin` date NOT NULL,
  `monto_mensual` decimal(10,2) NOT NULL,
  `fecha_finalizacion_anticipada` date DEFAULT NULL,
  `multa` decimal(10,2) DEFAULT NULL,
  `estado` enum('vigente','finalizado','rescindido') NOT NULL,
  PRIMARY KEY (`id_contrato`),
  KEY `fk_contratos_inmuebles_idx` (`id_inmueble`),
  KEY `fk_contratos_personas_idx` (`id_inquilino`),
  KEY `fk_contratos_usuarios_idx` (`id_usuario`),
  KEY `fk_contratos_usuarios_finaliza_idx` (`id_usuario_finaliza`),
  CONSTRAINT `fk_contratos_inmuebles` FOREIGN KEY (`id_inmueble`) REFERENCES `inmuebles` (`id_inmueble`),
  CONSTRAINT `fk_contratos_personas` FOREIGN KEY (`id_inquilino`) REFERENCES `personas` (`id_persona`),
  CONSTRAINT `fk_contratos_usuarios` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`),
  CONSTRAINT `fk_contratos_usuarios_finaliza` FOREIGN KEY (`id_usuario_finaliza`) REFERENCES `usuarios` (`id_usuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `contratos`
--

LOCK TABLES `contratos` WRITE;
/*!40000 ALTER TABLE `contratos` DISABLE KEYS */;
/*!40000 ALTER TABLE `contratos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inmuebles`
--

DROP TABLE IF EXISTS `inmuebles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inmuebles` (
  `id_inmueble` int NOT NULL AUTO_INCREMENT,
  `direccion` varchar(200) NOT NULL,
  `uso` enum('comercial','residencial') NOT NULL,
  `ambientes` int NOT NULL,
  `coordenadas` varchar(150) NOT NULL,
  `precio_base` decimal(10,2) NOT NULL,
  `estado` tinyint NOT NULL DEFAULT '1',
  `id_propietario` int NOT NULL,
  `id_tipo` int NOT NULL,
  `imagen` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id_inmueble`),
  UNIQUE KEY `direccion_UNIQUE` (`direccion`),
  UNIQUE KEY `coordenadas_UNIQUE` (`coordenadas`),
  KEY `fk_inmuebles_personas_idx` (`id_propietario`),
  KEY `fk_inmuebles_tipos_idx` (`id_tipo`),
  CONSTRAINT `fk_inmuebles_personas` FOREIGN KEY (`id_propietario`) REFERENCES `personas` (`id_persona`),
  CONSTRAINT `fk_inmuebles_tipos` FOREIGN KEY (`id_tipo`) REFERENCES `tipos_inmueble` (`id_tipo`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inmuebles`
--

LOCK TABLES `inmuebles` WRITE;
/*!40000 ALTER TABLE `inmuebles` DISABLE KEYS */;
INSERT INTO `inmuebles` VALUES (2,'Av Siempre Viva','comercial',2,'-33.268152921081274, -66.31631132258748',2000.00,1,1,1,'/uploads/inmuebles/inmueble_1_20251109234642.png'),(6,'Casa de piedra','residencial',2,'-33.27360682245227, -66.32701870128125',2000.00,1,3,1,'/uploads/inmuebles/inmueble_3_20251110024427.png');
/*!40000 ALTER TABLE `inmuebles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pagos`
--

DROP TABLE IF EXISTS `pagos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pagos` (
  `id_pago` int NOT NULL AUTO_INCREMENT,
  `id_contrato` int NOT NULL,
  `id_usuario` int NOT NULL,
  `fecha_pago` date NOT NULL,
  `numero_pago` varchar(45) NOT NULL,
  `importe` decimal(10,2) NOT NULL,
  `concepto` varchar(100) NOT NULL,
  `estado_pago` enum('aprobado','anulado') NOT NULL,
  PRIMARY KEY (`id_pago`),
  KEY `fk_pagos_contratos_idx` (`id_contrato`),
  KEY `fk_pagos_usuarios_idx` (`id_usuario`),
  CONSTRAINT `fk_pagos_contratos` FOREIGN KEY (`id_contrato`) REFERENCES `contratos` (`id_contrato`),
  CONSTRAINT `fk_pagos_usuarios` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pagos`
--

LOCK TABLES `pagos` WRITE;
/*!40000 ALTER TABLE `pagos` DISABLE KEYS */;
/*!40000 ALTER TABLE `pagos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `personas`
--

DROP TABLE IF EXISTS `personas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `personas` (
  `id_persona` int NOT NULL AUTO_INCREMENT,
  `dni` varchar(45) NOT NULL,
  `apellido` varchar(45) NOT NULL,
  `nombre` varchar(45) NOT NULL,
  `telefono` varchar(45) NOT NULL,
  `email` varchar(45) NOT NULL,
  `estado` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`id_persona`),
  UNIQUE KEY `dni_UNIQUE` (`dni`),
  UNIQUE KEY `email_UNIQUE` (`email`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `personas`
--

LOCK TABLES `personas` WRITE;
/*!40000 ALTER TABLE `personas` DISABLE KEYS */;
INSERT INTO `personas` VALUES (1,'35475532','SoriaUp','Cristian Josee','2664383838','correo@correo.com',1),(2,'35382736','Rosales','Adrian','2664838392','arosales@correo.com',1),(3,'21377171','Gonzalez','Francisco','2664353535','fgonzalez@correo.com',1),(4,'33828888','Perez','Gisela','2664000000','gperez@correo.com',1);
/*!40000 ALTER TABLE `personas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `personas_roles`
--

DROP TABLE IF EXISTS `personas_roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `personas_roles` (
  `id_persona` int NOT NULL,
  `id_rol` int NOT NULL,
  `fecha_alta` datetime DEFAULT CURRENT_TIMESTAMP,
  `fecha_baja` datetime DEFAULT NULL,
  `estado` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`id_persona`,`id_rol`),
  KEY `fk_personas_roles_personas_idx` (`id_persona`),
  KEY `fk_personas_roles_roles_idx` (`id_rol`),
  CONSTRAINT `fk_personas_roles_personas` FOREIGN KEY (`id_persona`) REFERENCES `personas` (`id_persona`),
  CONSTRAINT `fk_personas_roles_roles` FOREIGN KEY (`id_rol`) REFERENCES `roles` (`id_rol`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `personas_roles`
--

LOCK TABLES `personas_roles` WRITE;
/*!40000 ALTER TABLE `personas_roles` DISABLE KEYS */;
INSERT INTO `personas_roles` VALUES (1,1,'2025-11-07 13:56:58',NULL,1),(1,3,'2025-11-07 13:56:58',NULL,1),(2,2,'2025-11-07 12:20:45',NULL,1),(3,3,'2025-11-09 21:11:57',NULL,1),(4,4,'2025-11-13 11:14:22','2025-11-13 11:44:15',0);
/*!40000 ALTER TABLE `personas_roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `id_rol` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) NOT NULL,
  `descripcion` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id_rol`),
  UNIQUE KEY `nombre_UNIQUE` (`nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'ADMINISTRADOR','Administrador del sistema'),(2,'EMPLEADO','Empleado de la inmobiliaria'),(3,'PROPIETARIO','Dueño de inmuebles'),(4,'INQUILINO','Arrendatario de inmuebles');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tipos_inmueble`
--

DROP TABLE IF EXISTS `tipos_inmueble`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tipos_inmueble` (
  `id_tipo` int NOT NULL AUTO_INCREMENT,
  `descripcion` varchar(45) NOT NULL,
  PRIMARY KEY (`id_tipo`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tipos_inmueble`
--

LOCK TABLES `tipos_inmueble` WRITE;
/*!40000 ALTER TABLE `tipos_inmueble` DISABLE KEYS */;
INSERT INTO `tipos_inmueble` VALUES (1,'Casa'),(2,'Departamento'),(3,'Local comercial'),(4,'Oficina'),(5,'Terreno'),(6,'Galpón');
/*!40000 ALTER TABLE `tipos_inmueble` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `id_usuario` int NOT NULL AUTO_INCREMENT,
  `id_persona` int NOT NULL,
  `password` varchar(255) NOT NULL,
  `avatar` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id_usuario`),
  KEY `fk_usuarios_personas_idx` (`id_persona`),
  CONSTRAINT `fk_usuarios_personas` FOREIGN KEY (`id_persona`) REFERENCES `personas` (`id_persona`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
INSERT INTO `usuarios` VALUES (2,1,'$2a$11$B24gb3UcuNd9qTtQ/z5RJOW7uBG1coB1vW7J75WSsRgrk44zrVKoO','/uploads/avatars/avatar_1.jpg'),(3,2,'$2a$11$5CCa598syCnnKkAHBSXhiusf818Mvx7vHZwQ7iaqmVvDdNBom3dsm',NULL),(4,3,'$2a$11$xUosq6a3bcOUN9U7E2Jkg.q2ZvOyAr8xQ58t2VvlsoQ7HKDqM16wG',NULL);
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-01-30  6:03:34
