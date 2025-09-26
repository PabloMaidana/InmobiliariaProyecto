-- SQL script para crear la base de datos 'inmobiliaria' y tablas principales
-- Incluye seed de usuarios: administrador y empleado.
-- NOTA: las contraseñas ya están hasheadas con BCrypt (cost 10).
CREATE DATABASE IF NOT EXISTS `inmobiliaria` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `inmobiliaria`;

-- Tabla usuarios
DROP TABLE IF EXISTS `usuarios`;
CREATE TABLE `usuarios` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `email` VARCHAR(200) NOT NULL UNIQUE,
  `password_hash` VARCHAR(200) NOT NULL,
  `nombre` VARCHAR(100) NOT NULL,
  `apellido` VARCHAR(100) NOT NULL,
  `rol` VARCHAR(50) NOT NULL, -- 'Admin' o 'Empleado'
  `avatar_url` VARCHAR(500) NULL,
  `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Tablas de dominio (simplificadas, agregá las columnas que necesites)
DROP TABLE IF EXISTS `propietarios`;
CREATE TABLE `propietarios` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `dni` VARCHAR(20) NOT NULL UNIQUE,
  `nombre` VARCHAR(100) NOT NULL,
  `apellido` VARCHAR(100) NOT NULL,
  `telefono` VARCHAR(50),
  `email` VARCHAR(200)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `tipos_inmueble`;
CREATE TABLE `tipos_inmueble` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `nombre` VARCHAR(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `inmuebles`;
CREATE TABLE `inmuebles` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `propietario_id` INT NOT NULL,
  `direccion` VARCHAR(300) NOT NULL,
  `uso` VARCHAR(50) NOT NULL,
  `tipo_id` INT NOT NULL,
  `ambientes` INT,
  `precio` DECIMAL(12,2),
  `disponible` TINYINT(1) DEFAULT 1,
  `lat` DECIMAL(10,7) DEFAULT NULL,
  `lng` DECIMAL(10,7) DEFAULT NULL,
  FOREIGN KEY (`propietario_id`) REFERENCES `propietarios`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`tipo_id`) REFERENCES `tipos_inmueble`(`id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `inquilinos`;
CREATE TABLE `inquilinos` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `dni` VARCHAR(20) NOT NULL UNIQUE,
  `nombre` VARCHAR(200) NOT NULL,
  `telefono` VARCHAR(50),
  `email` VARCHAR(200)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `contratos`;
CREATE TABLE `contratos` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `inmueble_id` INT NOT NULL,
  `inquilino_id` INT NOT NULL,
  `monto` DECIMAL(12,2) NOT NULL,
  `fecha_inicio` DATE NOT NULL,
  `fecha_fin` DATE NOT NULL,
  `finalizado_anticipado_en` DATE DEFAULT NULL,
  `multa` DECIMAL(12,2) DEFAULT NULL,
  `creado_por` INT DEFAULT NULL,
  `terminado_por` INT DEFAULT NULL,
  FOREIGN KEY (`inmueble_id`) REFERENCES `inmuebles`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`inquilino_id`) REFERENCES `inquilinos`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`creado_por`) REFERENCES `usuarios`(`id`) ON DELETE SET NULL,
  FOREIGN KEY (`terminado_por`) REFERENCES `usuarios`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE IF EXISTS `pagos`;
CREATE TABLE `pagos` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `contrato_id` INT NOT NULL,
  `numero` INT NOT NULL,
  `fecha` DATE NOT NULL,
  `concepto` VARCHAR(300),
  `importe` DECIMAL(12,2) NOT NULL,
  `anulado` TINYINT(1) DEFAULT 0,
  `creado_por` INT DEFAULT NULL,
  `anulado_por` INT DEFAULT NULL,
  FOREIGN KEY (`contrato_id`) REFERENCES `contratos`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`creado_por`) REFERENCES `usuarios`(`id`) ON DELETE SET NULL,
  FOREIGN KEY (`anulado_por`) REFERENCES `usuarios`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Seeds básicos
INSERT INTO `usuarios` (email, password_hash, nombre, apellido, rol) VALUES
  ('admin@gmail.com', '$2b$10$szEVuPGuliK5anrbSc7A2.naudrU1xmKdI8yTwxMvKeq2iCGavsb6', 'Admin', 'Sistema', 'Admin'),
  ('empleado@gmail.com', '$2b$10$9q0NsSSz39UIMZd6rn/go.ALzKAgaES2oRxOIyDCrUZFOaurwKVFm', 'Empleado', 'Prueba', 'Empleado');

-- Algunos tipos y datos de ejemplo
INSERT INTO `tipos_inmueble` (nombre) VALUES ('Departamento'), ('Casa'), ('Local'), ('Depósito');

INSERT INTO `propietarios` (dni, nombre, apellido, telefono, email) VALUES
  ('20123456', 'Juan', 'Gomez', '2211234567', 'juan@example.com'),
  ('21123456', 'Ana', 'Rossi', '2217654321', 'ana@example.com');

-- Inmuebles ejemplo (referencian tipos y propietarios creados arriba)
INSERT INTO `inmuebles` (propietario_id, direccion, uso, tipo_id, ambientes, precio, disponible) VALUES
  (1, 'Mitre 123', 'Residencial', 1, 2, 150000.00, 1),
  (2, 'Av. Centro 500', 'Comercial', 3, 1, 350000.00, 1);

-- Inquilinos y contratos de ejemplo
INSERT INTO `inquilinos` (dni, nombre, telefono, email) VALUES
  ('33123456', 'Maria Perez', '2219988776', 'maria@example.com'),
  ('30999888', 'Luis Lopez', '2215544332', 'luis@example.com');

-- Contrato ejemplo
INSERT INTO `contratos` (inmueble_id, inquilino_id, monto, fecha_inicio, fecha_fin, creado_por) VALUES
  (1, 1, 150000.00, '2025-01-01', '2025-12-31', 1);

-- Pagos ejemplo
INSERT INTO `pagos` (contrato_id, numero, fecha, concepto, importe, creado_por) VALUES
  (1, 1, '2025-02-01', 'Alquiler febrero', 150000.00, 1);

-- Fin del script
