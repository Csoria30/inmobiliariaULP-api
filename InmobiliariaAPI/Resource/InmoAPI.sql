SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

CREATE SCHEMA IF NOT EXISTS `inmobiliariaulp` DEFAULT CHARACTER SET utf8mb3;
USE `inmobiliariaulp`;

-- -----------------------------------------------------
-- Table personas
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `personas` (
    `id_persona` INT NOT NULL AUTO_INCREMENT,
    `dni` VARCHAR(20) NOT NULL,
    `apellido` VARCHAR(45) NOT NULL,
    `nombre` VARCHAR(45) NOT NULL,
    `telefono` VARCHAR(45) NOT NULL,
    `email` VARCHAR(45) NOT NULL,
    `estado` TINYINT NOT NULL DEFAULT 1,
    PRIMARY KEY (`id_persona`),
    UNIQUE INDEX `dni_UNIQUE` (`dni` ASC),
    UNIQUE INDEX `email_UNIQUE` (`email` ASC)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb3;

-- -----------------------------------------------------
-- Table roles
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `roles` (
    `id_rol` INT NOT NULL AUTO_INCREMENT,
    `nombre` VARCHAR(50) NOT NULL,
    `descripcion` VARCHAR(255),
    PRIMARY KEY (`id_rol`),
    UNIQUE INDEX `nombre_UNIQUE` (`nombre` ASC)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb3;

-- -----------------------------------------------------
-- Table personas_roles
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `personas_roles` (
    `id_persona` INT NOT NULL,
    `id_rol` INT NOT NULL,
    `fecha_alta` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `fecha_baja` DATETIME NULL,
    `estado` TINYINT NOT NULL DEFAULT 1,
    PRIMARY KEY (`id_persona`, `id_rol`),
    INDEX `fk_personas_roles_personas_idx` (`id_persona` ASC),
    INDEX `fk_personas_roles_roles_idx` (`id_rol` ASC),
    CONSTRAINT `fk_personas_roles_personas`
        FOREIGN KEY (`id_persona`)
        REFERENCES `personas` (`id_persona`),
    CONSTRAINT `fk_personas_roles_roles`
        FOREIGN KEY (`id_rol`)
        REFERENCES `roles` (`id_rol`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb3;

-- -----------------------------------------------------
-- Table usuarios
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `usuarios` (
    `id_usuario` INT NOT NULL AUTO_INCREMENT,
    `id_persona` INT NOT NULL,
    `password` VARCHAR(255) NOT NULL,
    `avatar` VARCHAR(255) NULL,
    PRIMARY KEY (`id_usuario`),
    INDEX `fk_usuarios_personas_idx` (`id_persona` ASC),
    CONSTRAINT `fk_usuarios_personas`
        FOREIGN KEY (`id_persona`)
        REFERENCES `personas` (`id_persona`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb3;

-- -----------------------------------------------------
-- Table tipos_inmueble
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `tipos_inmueble` (
    `id_tipo` INT NOT NULL AUTO_INCREMENT,
    `descripcion` VARCHAR(45) NOT NULL,
    PRIMARY KEY (`id_tipo`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb3;

-- -----------------------------------------------------
-- Table inmuebles
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmuebles` (
    `id_inmueble` INT NOT NULL AUTO_INCREMENT,
    `direccion` VARCHAR(200) NOT NULL,
    `uso` ENUM('comercial', 'residencial') NOT NULL,
    `ambientes` INT NOT NULL,
    `coordenadas` VARCHAR(150) NOT NULL,
    `precio_base` DECIMAL(10,2) NOT NULL,
    `estado` TINYINT NOT NULL DEFAULT 1,
    `id_propietario` INT NOT NULL,
    `id_tipo` INT NOT NULL,
    PRIMARY KEY (`id_inmueble`),
    UNIQUE INDEX `direccion_UNIQUE` (`direccion` ASC),
    UNIQUE INDEX `coordenadas_UNIQUE` (`coordenadas` ASC),
    INDEX `fk_inmuebles_personas_idx` (`id_propietario` ASC),
    INDEX `fk_inmuebles_tipos_idx` (`id_tipo` ASC),
    CONSTRAINT `fk_inmuebles_personas`
        FOREIGN KEY (`id_propietario`)
        REFERENCES `personas` (`id_persona`),
    CONSTRAINT `fk_inmuebles_tipos`
        FOREIGN KEY (`id_tipo`)
        REFERENCES `tipos_inmueble` (`id_tipo`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb3;

-- -----------------------------------------------------
-- Table contratos
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `contratos` (
    `id_contrato` INT NOT NULL AUTO_INCREMENT,
    `id_inmueble` INT NOT NULL,
    `id_inquilino` INT NOT NULL,
    `id_usuario` INT NOT NULL,
    `id_usuario_finaliza` INT NULL,
    `fecha_inicio` DATE NOT NULL,
    `fecha_fin` DATE NOT NULL,
    `monto_mensual` DECIMAL(10,2) NOT NULL,
    `fecha_finalizacion_anticipada` DATE NULL,
    `multa` DECIMAL(10,2) NULL,
    `estado` ENUM('vigente', 'finalizado', 'rescindido') NOT NULL,
    PRIMARY KEY (`id_contrato`),
    INDEX `fk_contratos_inmuebles_idx` (`id_inmueble` ASC),
    INDEX `fk_contratos_personas_idx` (`id_inquilino` ASC),
    INDEX `fk_contratos_usuarios_idx` (`id_usuario` ASC),
    INDEX `fk_contratos_usuarios_finaliza_idx` (`id_usuario_finaliza` ASC),
    CONSTRAINT `fk_contratos_inmuebles`
        FOREIGN KEY (`id_inmueble`)
        REFERENCES `inmuebles` (`id_inmueble`),
    CONSTRAINT `fk_contratos_personas`
        FOREIGN KEY (`id_inquilino`)
        REFERENCES `personas` (`id_persona`),
    CONSTRAINT `fk_contratos_usuarios`
        FOREIGN KEY (`id_usuario`)
        REFERENCES `usuarios` (`id_usuario`),
    CONSTRAINT `fk_contratos_usuarios_finaliza`
        FOREIGN KEY (`id_usuario_finaliza`)
        REFERENCES `usuarios` (`id_usuario`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb3;

-- -----------------------------------------------------
-- Table pagos
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `pagos` (
    `id_pago` INT NOT NULL AUTO_INCREMENT,
    `id_contrato` INT NOT NULL,
    `id_usuario` INT NOT NULL,
    `fecha_pago` DATE NOT NULL,
    `numero_pago` VARCHAR(45) NOT NULL,
    `importe` DECIMAL(10,2) NOT NULL,
    `concepto` VARCHAR(100) NOT NULL,
    `estado_pago` ENUM('aprobado', 'anulado') NOT NULL,
    PRIMARY KEY (`id_pago`),
    INDEX `fk_pagos_contratos_idx` (`id_contrato` ASC),
    INDEX `fk_pagos_usuarios_idx` (`id_usuario` ASC),
    CONSTRAINT `fk_pagos_contratos`
        FOREIGN KEY (`id_contrato`)
        REFERENCES `contratos` (`id_contrato`),
    CONSTRAINT `fk_pagos_usuarios`
        FOREIGN KEY (`id_usuario`)
        REFERENCES `usuarios` (`id_usuario`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb3;

-- -----------------------------------------------------
-- Datos iniciales
-- -----------------------------------------------------
INSERT INTO roles (nombre, descripcion) VALUES
('ADMINISTRADOR', 'Administrador del sistema'),
('EMPLEADO', 'Empleado de la inmobiliaria'),
('PROPIETARIO', 'Dueño de inmuebles'),
('INQUILINO', 'Arrendatario de inmuebles');

INSERT INTO tipos_inmueble (descripcion) VALUES
('Casa'),
('Departamento'),
('Local comercial'),
('Oficina'),
('Terreno'),
('Galpón');
