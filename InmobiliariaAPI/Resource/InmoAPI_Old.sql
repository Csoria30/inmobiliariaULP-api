-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------
-- -----------------------------------------------------
-- Schema inmobiliariaulp
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema inmobiliariaulp
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `inmobiliariaulp` DEFAULT CHARACTER SET utf8mb3 ;
USE `inmobiliariaulp` ;

-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`personas`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`personas` (
  `id_persona` INT NOT NULL AUTO_INCREMENT,
  `dni` VARCHAR(45) NOT NULL,
  `apellido` VARCHAR(45) NOT NULL,
  `nombre` VARCHAR(45) NOT NULL,
  `telefono` VARCHAR(45) NOT NULL,
  `email` VARCHAR(45) NOT NULL,
  `estado` TINYINT NOT NULL DEFAULT '1',
  PRIMARY KEY (`id_persona`),
  UNIQUE INDEX `dni_UNIQUE` (`dni` ASC) VISIBLE,
  UNIQUE INDEX `email_UNIQUE` (`email` ASC) VISIBLE)
ENGINE = InnoDB
AUTO_INCREMENT = 16
DEFAULT CHARACTER SET = utf8mb3;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`propietarios`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`propietarios` (
  `id_propietario` INT NOT NULL AUTO_INCREMENT,
  `id_persona` INT NOT NULL,
  `estado` TINYINT NOT NULL DEFAULT '1',
  PRIMARY KEY (`id_propietario`),
  INDEX `fk_Propietario_Persona1_idx` (`id_persona` ASC) VISIBLE,
  CONSTRAINT `fk_Propietario_Persona1`
    FOREIGN KEY (`id_persona`)
    REFERENCES `inmobiliariaulp`.`personas` (`id_persona`))
ENGINE = InnoDB
AUTO_INCREMENT = 7
DEFAULT CHARACTER SET = utf8mb3;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`tipos`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`tipos` (
  `id_tipo` INT NOT NULL AUTO_INCREMENT,
  `descripcion` VARCHAR(45) NOT NULL,
  PRIMARY KEY (`id_tipo`))
ENGINE = InnoDB
AUTO_INCREMENT = 12
DEFAULT CHARACTER SET = utf8mb3;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`inmuebles`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`inmuebles` (
  `id_inmueble` INT NOT NULL AUTO_INCREMENT,
  `direccion` VARCHAR(200) NOT NULL,
  `uso` ENUM('comercial', 'residencial') NOT NULL,
  `ambientes` INT NOT NULL,
  `coordenadas` VARCHAR(150) NOT NULL,
  `precio_base` DECIMAL(10,0) NOT NULL,
  `estado` TINYINT NOT NULL DEFAULT '1',
  `id_propietario` INT NOT NULL,
  `id_tipo` INT NOT NULL,
  PRIMARY KEY (`id_inmueble`),
  UNIQUE INDEX `direccion_UNIQUE` (`direccion` ASC) VISIBLE,
  UNIQUE INDEX `coordenadas_UNIQUE` (`coordenadas` ASC) VISIBLE,
  INDEX `fk_Inmueble_Propietario1_idx` (`id_propietario` ASC) VISIBLE,
  INDEX `fk_Inmueble_Tipo1_idx` (`id_tipo` ASC) VISIBLE,
  CONSTRAINT `fk_Inmueble_Propietario1`
    FOREIGN KEY (`id_propietario`)
    REFERENCES `inmobiliariaulp`.`propietarios` (`id_propietario`),
  CONSTRAINT `fk_Inmueble_Tipo1`
    FOREIGN KEY (`id_tipo`)
    REFERENCES `inmobiliariaulp`.`tipos` (`id_tipo`))
ENGINE = InnoDB
AUTO_INCREMENT = 13
DEFAULT CHARACTER SET = utf8mb3;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`inquilinos`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`inquilinos` (
  `id_inquilino` INT NOT NULL AUTO_INCREMENT,
  `id_persona` INT NOT NULL,
  `estado` TINYINT NOT NULL DEFAULT '1',
  PRIMARY KEY (`id_inquilino`),
  INDEX `fk_inquilinos_personas1_idx` (`id_persona` ASC) VISIBLE,
  CONSTRAINT `fk_inquilinos_personas1`
    FOREIGN KEY (`id_persona`)
    REFERENCES `inmobiliariaulp`.`personas` (`id_persona`))
ENGINE = InnoDB
AUTO_INCREMENT = 8
DEFAULT CHARACTER SET = utf8mb3;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`empleados`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`empleados` (
  `id_empleado` INT NOT NULL AUTO_INCREMENT,
  `id_persona` INT NOT NULL,
  `estado` TINYINT NOT NULL DEFAULT '1',
  PRIMARY KEY (`id_empleado`),
  INDEX `fk_Empleado_Persona1_idx` (`id_persona` ASC) VISIBLE,
  CONSTRAINT `fk_Empleado_Persona1`
    FOREIGN KEY (`id_persona`)
    REFERENCES `inmobiliariaulp`.`personas` (`id_persona`))
ENGINE = InnoDB
AUTO_INCREMENT = 4
DEFAULT CHARACTER SET = utf8mb3;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`usuarios`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`usuarios` (
  `id_usuario` INT NOT NULL AUTO_INCREMENT,
  `id_empleado` INT NOT NULL,
  `password` VARCHAR(45) NOT NULL,
  `rol` ENUM('administrador', 'empleado') NOT NULL,
  `avatar` VARCHAR(45) NULL DEFAULT NULL,
  PRIMARY KEY (`id_usuario`),
  INDEX `fk_usuarios_empleados1_idx` (`id_empleado` ASC) VISIBLE,
  CONSTRAINT `fk_usuarios_empleados1`
    FOREIGN KEY (`id_empleado`)
    REFERENCES `inmobiliariaulp`.`empleados` (`id_empleado`))
ENGINE = InnoDB
AUTO_INCREMENT = 4
DEFAULT CHARACTER SET = utf8mb3;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`contratos`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`contratos` (
  `id_contrato` INT NOT NULL AUTO_INCREMENT,
  `id_inmueble` INT NOT NULL,
  `id_inquilino` INT NOT NULL,
  `id_usuario` INT NOT NULL,
  `id_usuario_finaliza` INT NULL DEFAULT NULL,
  `fecha_inicio` DATE NOT NULL,
  `fecha_fin` DATE NOT NULL,
  `monto_mensual` DECIMAL(10,0) NOT NULL,
  `fecha_finalizacion_anticipada` DATE NULL DEFAULT NULL,
  `multa` DECIMAL(10,0) NULL DEFAULT NULL,
  `estado` ENUM('vigente', 'finalizado', 'rescindido') NOT NULL,
  PRIMARY KEY (`id_contrato`),
  INDEX `fk_Contrato_Inquilino1_idx` (`id_inquilino` ASC) VISIBLE,
  INDEX `fk_Contrato_Inmueble1_idx` (`id_inmueble` ASC) VISIBLE,
  INDEX `fk_contratos_usuarios1_idx` (`id_usuario` ASC) VISIBLE,
  INDEX `fk_contratos_usuarios_finaliza` (`id_usuario_finaliza` ASC) VISIBLE,
  CONSTRAINT `fk_Contrato_Inmueble1`
    FOREIGN KEY (`id_inmueble`)
    REFERENCES `inmobiliariaulp`.`inmuebles` (`id_inmueble`),
  CONSTRAINT `fk_Contrato_Inquilino1`
    FOREIGN KEY (`id_inquilino`)
    REFERENCES `inmobiliariaulp`.`inquilinos` (`id_inquilino`),
  CONSTRAINT `fk_contratos_usuarios1`
    FOREIGN KEY (`id_usuario`)
    REFERENCES `inmobiliariaulp`.`usuarios` (`id_usuario`),
  CONSTRAINT `fk_contratos_usuarios_finaliza`
    FOREIGN KEY (`id_usuario_finaliza`)
    REFERENCES `inmobiliariaulp`.`usuarios` (`id_usuario`))
ENGINE = InnoDB
AUTO_INCREMENT = 7
DEFAULT CHARACTER SET = utf8mb3;


-- -----------------------------------------------------
-- Table `inmobiliariaulp`.`pagos`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `inmobiliariaulp`.`pagos` (
  `id_pago` INT NOT NULL AUTO_INCREMENT,
  `id_contrato` INT NOT NULL,
  `id_usuario` INT NOT NULL,
  `fecha_pago` DATE NOT NULL,
  `numero_pago` VARCHAR(45) NOT NULL,
  `importe` DECIMAL(10,0) NOT NULL,
  `concepto` VARCHAR(100) NOT NULL,
  `estadoPago` ENUM('aprobado', 'anulado') NOT NULL,
  PRIMARY KEY (`id_pago`),
  INDEX `fk_Pago_Contrato_idx` (`id_contrato` ASC) VISIBLE,
  INDEX `fk_pagos_usuarios1_idx` (`id_usuario` ASC) VISIBLE,
  CONSTRAINT `fk_Pago_Contrato`
    FOREIGN KEY (`id_contrato`)
    REFERENCES `inmobiliariaulp`.`contratos` (`id_contrato`),
  CONSTRAINT `fk_pagos_usuarios1`
    FOREIGN KEY (`id_usuario`)
    REFERENCES `inmobiliariaulp`.`usuarios` (`id_usuario`))
ENGINE = InnoDB
AUTO_INCREMENT = 4
DEFAULT CHARACTER SET = utf8mb3;

USE `inmobiliariaulp` ;

-- -----------------------------------------------------
-- procedure sp_DeleteEmpleado
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_DeleteEmpleado`(
    IN p_empleado_id INT
)
BEGIN
    DELETE FROM empleados 
    WHERE id_empleado = p_empleado_id;
    
    -- Retorna el número de filas afectadas
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_DeleteInquilino
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_DeleteInquilino`(
    IN p_inquilino_id INT
)
BEGIN
    DELETE FROM inquilinos 
    WHERE id_inquilino = p_inquilino_id;
    
    -- Retorna el número de filas afectadas
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_DeletePropietario
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_DeletePropietario`(
    IN p_propietario_id INT
)
BEGIN
    DELETE FROM propietarios 
    WHERE id_propietario = p_propietario_id;
    
    -- Retorna el número de filas afectadas
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_GetContratoDetalle
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GetContratoDetalle`(
    IN p_contrato_id INT
)
BEGIN
    SELECT 
        c.id_contrato AS ContratoId,
        -- Inmueble
        c.id_inmueble AS InmuebleId,
        i.direccion AS Direccion,
        t.descripcion AS TipoInmueble,
        i.uso AS UsoInmueble,
        i.ambientes AS Ambientes,
        i.coordenadas AS Coordenadas,
        i.estado AS EstadoInmueble,
        
        -- Propietario
        pro.id_propietario AS PropietarioId,
        pro.id_persona AS PropietarioIdPersona,
        CONCAT(p.apellido, ' ', p.nombre) AS NombrePropietario,
        p.email AS EmailPropietario,
        p.telefono AS TelefonoPropietario,
        
        -- Inquilino
        c.id_inquilino AS InquilinoId,
        pi.id_persona AS InquilinoIdPersona,
        CONCAT(pi.apellido, ' ', pi.nombre) AS NombreInquilino,
        pi.email AS EmailInquilino,
        pi.telefono AS TelefonoInquilino,
        
        -- Usuario Inicio
        c.id_usuario AS UsuarioId,
        CONCAT(pu.apellido, ' ', pu.nombre) AS NombreEmpleado,
        pu.email AS EmailUsuario,
        u.rol AS RolUsuario,
        
        -- Usuario Fin
        c.id_usuario_finaliza AS UsuarioIdFin,
        CONCAT(puf.apellido, ' ', puf.nombre) AS NombreEmpleadoFin,
        puf.email AS EmailUsuarioFin,
        uf.rol AS RolUsuarioFin,
        
        -- Contrato
        c.fecha_inicio AS FechaInicio,
        c.fecha_fin AS FechaFin,
        c.monto_mensual AS MontoMensual,
        c.fecha_finalizacion_anticipada AS FechaAnticipada,
        c.multa AS Multa,
        c.estado AS EstadoContrato,
        
        -- Pagos
        (SELECT COUNT(*) FROM pagos pg WHERE pg.id_contrato = c.id_contrato AND pg.estadoPago = 'aprobado') AS PagosRealizados

    FROM contratos c
        JOIN inmuebles i ON i.id_inmueble = c.id_inmueble
        JOIN tipos t ON i.id_tipo = t.id_tipo
        JOIN propietarios pro ON i.id_propietario = pro.id_propietario
        JOIN personas p ON pro.id_persona = p.id_persona
        JOIN inquilinos inq ON inq.id_inquilino = c.id_inquilino
        JOIN personas pi ON inq.id_persona = pi.id_persona
        JOIN usuarios u ON c.id_usuario = u.id_usuario
        JOIN empleados e ON u.id_empleado = e.id_empleado
        JOIN personas pu ON e.id_persona = pu.id_persona
        LEFT JOIN usuarios uf ON c.id_usuario_finaliza = uf.id_usuario
        LEFT JOIN empleados ef ON uf.id_empleado = ef.id_empleado
        LEFT JOIN personas puf ON ef.id_persona = puf.id_persona
    WHERE c.id_contrato = p_contrato_id;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_GetEmpleadoById
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GetEmpleadoById`(
    IN p_empleado_id INT
)
BEGIN
    SELECT 
        id_empleado,
        id_persona, 
        estado 
    FROM empleados
    WHERE id_empleado = p_empleado_id;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_GetInmuebleById
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GetInmuebleById`(
    IN p_inmueble_id INT
)
BEGIN
    SELECT 	
        i.id_inmueble, 
        i.direccion, 
        i.uso, 
        i.ambientes, 
        i.coordenadas, 
        i.precio_base, 
        i.estado, 
        i.id_propietario,
        p.nombre,
        p.apellido,
        i.id_tipo, 
        t.descripcion

    FROM inmuebles i
        JOIN propietarios pr
            ON pr.id_propietario = i.id_propietario
        JOIN personas p
            ON p.id_persona = pr.id_persona
        JOIN tipos t
            ON i.id_tipo = t.id_tipo

    WHERE i.id_inmueble = p_inmueble_id;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_GetInquilinoById
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GetInquilinoById`(
    IN p_persona_id INT
)
BEGIN
    SELECT 
        id_inquilino,
        id_persona, 
        estado 
    FROM inquilinos
    WHERE id_persona = p_persona_id;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_GetPropietarioById
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GetPropietarioById`(
    IN p_propietario_id INT
)
BEGIN
    SELECT 
        id_propietario, 
        id_persona, 
        estado 
    FROM propietarios 
    WHERE id_propietario = p_propietario_id;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_GetUsuarioByEmail
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GetUsuarioByEmail`(
    IN p_email VARCHAR(255)
)
BEGIN
    SELECT
        p.id_persona AS PersonaId,
        u.id_usuario AS UsuarioId,
        u.id_empleado AS EmpleadoId,
        u.password AS Password,
        u.rol AS Rol,
        u.avatar AS Avatar,
        e.estado AS Estado,
        p.email AS Email,
        p.apellido AS Apellido,
        p.nombre AS Nombre,
        p.telefono AS Telefono
                
    FROM usuarios u
        JOIN empleados e 
            ON u.id_empleado = e.id_empleado
        JOIN personas p
            ON e.id_persona = p.id_persona
                
    WHERE p.email = p_email;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_GetUsuarioById
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GetUsuarioById`(
    IN p_usuario_id INT
)
BEGIN
    SELECT
        p.id_persona AS PersonaId,
        u.id_usuario AS UsuarioId,
        u.id_empleado AS EmpleadoId,
        u.password AS Password,
        u.rol AS Rol,
        u.avatar AS Avatar,
        e.estado AS Estado,
        p.email AS Email,
        p.apellido AS Apellido,
        p.nombre AS Nombre,
        p.telefono AS Telefono
                
    FROM usuarios u
        JOIN empleados e 
            ON u.id_empleado = e.id_empleado
        JOIN personas p
            ON e.id_persona = p.id_persona
                
    WHERE u.id_usuario = p_usuario_id;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_InsertContrato
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_InsertContrato`(
    IN p_id_inmueble INT,
    IN p_id_inquilino INT,
    IN p_id_usuario INT,
    IN p_fecha_inicio DATE,
    IN p_fecha_fin DATE,
    IN p_monto_mensual DECIMAL(10,2),
    IN p_estado VARCHAR(20),
    OUT p_contrato_id INT
)
BEGIN
    INSERT INTO contratos(
        id_inmueble, 
        id_inquilino, 
        id_usuario, 
        fecha_inicio, 
        fecha_fin, 
        monto_mensual, 
        estado
    ) VALUES(
        p_id_inmueble, 
        p_id_inquilino, 
        p_id_usuario, 
        p_fecha_inicio, 
        p_fecha_fin, 
        p_monto_mensual, 
        p_estado
    );
    
    SET p_contrato_id = LAST_INSERT_ID();
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_InsertEmpleado
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_InsertEmpleado`(
    IN p_id_persona INT,
    OUT p_empleado_id INT
)
BEGIN
    INSERT INTO empleados (id_persona) VALUES (p_id_persona);
    SET p_empleado_id = LAST_INSERT_ID();
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_InsertInmuebleReturnId
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_InsertInmuebleReturnId`(
    IN p_direccion VARCHAR(255),
    IN p_uso VARCHAR(100),
    IN p_ambientes INT,
    IN p_coordenadas VARCHAR(255),
    IN p_precio_base DECIMAL(10,2),
    IN p_id_propietario INT,
    IN p_id_tipo INT
)
BEGIN
    INSERT INTO inmuebles (
        direccion, 
        uso, 
        ambientes, 
        coordenadas, 
        precio_base, 
        id_propietario, 
        id_tipo
    ) VALUES (
        p_direccion,
        p_uso,
        p_ambientes,
        p_coordenadas,
        p_precio_base,
        p_id_propietario,
        p_id_tipo
    );
    
    SELECT LAST_INSERT_ID() AS InmuebleId;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_InsertInquilino
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_InsertInquilino`(
    IN p_id_persona INT,
    OUT p_inquilino_id INT
)
BEGIN
    INSERT INTO inquilinos (id_persona) VALUES (p_id_persona);
    SET p_inquilino_id = LAST_INSERT_ID();
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_InsertPersona
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_InsertPersona`(
    IN p_dni VARCHAR(20),
    IN p_apellido VARCHAR(100),
    IN p_nombre VARCHAR(100),
    IN p_telefono VARCHAR(20),
    IN p_email VARCHAR(100),
    OUT p_id INT
)
BEGIN
    INSERT INTO personas (dni, apellido, nombre, telefono, email) 
    VALUES (p_dni, p_apellido, p_nombre, p_telefono, p_email);
    
    SET p_id = LAST_INSERT_ID();
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_InsertPropietario
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_InsertPropietario`(
    IN p_id_persona INT,
    OUT p_propietario_id INT
)
BEGIN
    INSERT INTO propietarios (id_persona) VALUES (p_id_persona);
    SET p_propietario_id = LAST_INSERT_ID();
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_InsertUsuario
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_InsertUsuario`(
    IN p_id_empleado INT,
    IN p_password VARCHAR(255),
    IN p_rol VARCHAR(50),
    IN p_avatar VARCHAR(255),
    OUT p_usuario_id INT
)
BEGIN
    INSERT INTO usuarios (id_empleado, password, rol, avatar) 
    VALUES (p_id_empleado, p_password, p_rol, p_avatar);
    
    SET p_usuario_id = LAST_INSERT_ID();
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_ListInmueblesActivos
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ListInmueblesActivos`(
    IN p_term VARCHAR(255)
)
BEGIN
    SELECT 	
        i.id_inmueble AS InmuebleId, 
        i.direccion AS Direccion, 
        i.uso AS Uso, 
        i.ambientes AS Ambientes, 
        i.coordenadas AS Coordenadas, 
        i.precio_base AS PrecioBase, 
        i.estado AS EstadoInmueble, 
        i.id_propietario AS PropietarioId, 
        CONCAT(p.apellido, ' ', p.nombre) AS NombrePropietario,
        i.id_tipo AS Tipo, 
        t.descripcion AS Descripcion,
        p.email AS Email,
        p.telefono AS Telefono

    FROM inmuebles i
        JOIN propietarios pr
            ON pr.id_propietario = i.id_propietario
        JOIN personas p
            ON p.id_persona = pr.id_persona
        JOIN tipos t
            ON i.id_tipo = t.id_tipo

    WHERE 
        (LOWER(i.direccion) LIKE LOWER(CONCAT('%', p_term, '%')) OR 
         LOWER(i.uso) LIKE LOWER(CONCAT('%', p_term, '%'))) AND 
        i.estado = 1
    ORDER BY i.direccion
    LIMIT 10;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_ListInquilinosActivos
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ListInquilinosActivos`(
    IN p_term VARCHAR(255)
)
BEGIN
    SELECT 
        i.id_inquilino AS InquilinoId,
        p.dni AS Dni,
        CONCAT(p.apellido, ' ', p.nombre) AS NombreInquilino,
        p.email AS Email,
        p.telefono AS Telefono

    FROM inquilinos i
        JOIN personas p 
            ON i.id_persona = p.id_persona

    WHERE i.estado = 1
      AND (p.apellido LIKE CONCAT('%', p_term, '%') OR 
           p.nombre LIKE CONCAT('%', p_term, '%') OR 
           p.dni LIKE CONCAT('%', p_term, '%'))
    
    ORDER BY p.apellido, p.nombre
    LIMIT 10;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_ListPropietariosActivos
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ListPropietariosActivos`(
    IN p_term VARCHAR(255)
)
BEGIN
    SELECT
        pr.id_propietario, 
        p.id_persona, 
        p.apellido, 
        p.nombre
    FROM personas p
        INNER JOIN propietarios pr 
            ON p.id_persona = pr.id_persona
    WHERE pr.estado = 1 
      AND (p.apellido LIKE CONCAT('%', p_term, '%') OR 
           p.nombre LIKE CONCAT('%', p_term, '%'))
    ORDER BY p.apellido, p.nombre
    LIMIT 10;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_UpdateContrato
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_UpdateContrato`(
    IN p_id_contrato INT,
    IN p_id_inmueble INT,
    IN p_id_inquilino INT,
    IN p_id_usuario INT,
    IN p_fecha_inicio DATE,
    IN p_fecha_fin DATE,
    IN p_monto_mensual DECIMAL(10,2),
    IN p_fecha_finalizacion_anticipada DATE,
    IN p_multa DECIMAL(10,2),
    IN p_estado VARCHAR(20)
)
BEGIN
    UPDATE contratos
    SET 
        id_inmueble = p_id_inmueble,
        id_inquilino = p_id_inquilino,
        id_usuario = p_id_usuario,
        fecha_inicio = p_fecha_inicio,
        fecha_fin = p_fecha_fin,
        monto_mensual = p_monto_mensual,
        fecha_finalizacion_anticipada = p_fecha_finalizacion_anticipada,
        multa = p_multa,
        estado = p_estado
    WHERE id_contrato = p_id_contrato;
    
    -- Retorna el número de filas afectadas
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_UpdateEmpleadoEstado
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_UpdateEmpleadoEstado`(
    IN p_empleado_id INT,
    IN p_estado TINYINT
)
BEGIN
    UPDATE empleados 
    SET estado = p_estado 
    WHERE id_empleado = p_empleado_id;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_UpdateInmueble
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_UpdateInmueble`(
    IN p_id_inmueble INT,
    IN p_direccion VARCHAR(255),
    IN p_uso VARCHAR(100),
    IN p_ambientes INT,
    IN p_coordenadas VARCHAR(255),
    IN p_precio_base DECIMAL(10,2),
    IN p_id_propietario INT,
    IN p_id_tipo INT
)
BEGIN
    UPDATE inmuebles
    SET 
        direccion = p_direccion,
        uso = p_uso,
        ambientes = p_ambientes,
        coordenadas = p_coordenadas,
        precio_base = p_precio_base,
        id_propietario = p_id_propietario,
        id_tipo = p_id_tipo
    WHERE id_inmueble = p_id_inmueble;
    
    -- Retorna el número de filas afectadas
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_UpdateInmuebleEstado
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_UpdateInmuebleEstado`(
    IN p_inmueble_id INT,
    IN p_estado TINYINT
)
BEGIN
    UPDATE inmuebles
    SET estado = p_estado
    WHERE id_inmueble = p_inmueble_id;
    
    -- Retorna el número de filas afectadas
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_UpdateInquilinoEstado
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_UpdateInquilinoEstado`(
    IN p_inquilino_id INT,
    IN p_estado TINYINT
)
BEGIN
    UPDATE inquilinos 
    SET estado = p_estado 
    WHERE id_inquilino = p_inquilino_id;
    
    -- Retorna el número de filas afectadas
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_UpdatePasswordByEmail
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_UpdatePasswordByEmail`(
    IN p_password VARCHAR(255),
    IN p_email VARCHAR(255)
)
BEGIN
    UPDATE usuarios u
    JOIN empleados e ON u.id_empleado = e.id_empleado
    JOIN personas p ON e.id_persona = p.id_persona
    SET u.password = p_password
    WHERE p.email = p_email;
    
    -- Retorna el número de filas afectadas
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_UpdatePropietarioEstado
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_UpdatePropietarioEstado`(
    IN p_propietario_id INT,
    IN p_estado TINYINT
)
BEGIN
    UPDATE propietarios 
    SET estado = p_estado 
    WHERE id_propietario = p_propietario_id;
    
    -- Retorna el número de filas afectadas
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure sp_UpdateUsuario
-- -----------------------------------------------------

DELIMITER $$
USE `inmobiliariaulp`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_UpdateUsuario`(
    IN p_id_empleado INT,
    IN p_password VARCHAR(255),
    IN p_rol VARCHAR(50)
)
BEGIN
    UPDATE usuarios
    SET password = p_password,
        rol = p_rol
    WHERE id_empleado = p_id_empleado;
    
    -- Retorna el número de filas afectadas
    SELECT ROW_COUNT() AS FilasAfectadas;
END$$

DELIMITER ;

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
