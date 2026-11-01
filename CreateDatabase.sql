-- ============================================================
--  CLINIQUE VÉTÉRINAIRE - Script de création (LocalDB .mdf)
--  Exécuter DIRECTEMENT dans la base CliniqueVet.mdf
--  NE PAS exécuter USE master / CREATE DATABASE (déjà fait via VS)
-- ============================================================

-- Suppression des tables si elles existent (ordre inverse des FK)
IF OBJECT_ID('ConsultationMedicaments','U') IS NOT NULL DROP TABLE ConsultationMedicaments;
IF OBJECT_ID('Factures','U')               IS NOT NULL DROP TABLE Factures;
IF OBJECT_ID('Vaccinations','U')           IS NOT NULL DROP TABLE Vaccinations;
IF OBJECT_ID('Consultations','U')          IS NOT NULL DROP TABLE Consultations;
IF OBJECT_ID('RendezVous','U')             IS NOT NULL DROP TABLE RendezVous;
IF OBJECT_ID('Animaux','U')               IS NOT NULL DROP TABLE Animaux;
IF OBJECT_ID('Proprietaires','U')         IS NOT NULL DROP TABLE Proprietaires;
IF OBJECT_ID('Utilisateurs','U')          IS NOT NULL DROP TABLE Utilisateurs;
IF OBJECT_ID('Medicaments','U')           IS NOT NULL DROP TABLE Medicaments;
GO

-- Suppression des vues si elles existent
IF OBJECT_ID('vw_PlanningVet','V')    IS NOT NULL DROP VIEW vw_PlanningVet;
IF OBJECT_ID('vw_StockFaible','V')    IS NOT NULL DROP VIEW vw_StockFaible;
IF OBJECT_ID('vw_DashboardStats','V') IS NOT NULL DROP VIEW vw_DashboardStats;
GO

-- ==================== TABLE UTILISATEURS ====================
CREATE TABLE Utilisateurs (
    IdUtilisateur   INT IDENTITY(1,1) PRIMARY KEY,
    Nom             NVARCHAR(100) NOT NULL,
    Prenom          NVARCHAR(100) NOT NULL,
    Login           NVARCHAR(50)  NOT NULL UNIQUE,
    MotDePasse      NVARCHAR(256) NOT NULL,
    Role            NVARCHAR(20)  NOT NULL CHECK (Role IN ('Administrateur','Secretaire','Veterinaire')),
    Telephone       NVARCHAR(20),
    Email           NVARCHAR(150),
    Actif           BIT           NOT NULL DEFAULT 1,
    DateCreation    DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ==================== TABLE PROPRIETAIRES ==================
CREATE TABLE Proprietaires (
    IdProprietaire  INT IDENTITY(1,1) PRIMARY KEY,
    Nom             NVARCHAR(100) NOT NULL,
    Prenom          NVARCHAR(100) NOT NULL,
    Adresse         NVARCHAR(255),
    Telephone       NVARCHAR(20),
    Email           NVARCHAR(150),
    DateInscription DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE)
);
GO

-- ===================== TABLE ANIMAUX =======================
CREATE TABLE Animaux (
    IdAnimal        INT IDENTITY(1,1) PRIMARY KEY,
    IdProprietaire  INT NOT NULL REFERENCES Proprietaires(IdProprietaire) ON DELETE CASCADE,
    Nom             NVARCHAR(100) NOT NULL,
    Espece          NVARCHAR(50)  NOT NULL,
    Race            NVARCHAR(100),
    Sexe            CHAR(1)       CHECK (Sexe IN ('M','F')),
    DateNaissance   DATE,
    Couleur         NVARCHAR(50),
    NumPuce         NVARCHAR(50),
    Actif           BIT           NOT NULL DEFAULT 1
);
GO

-- ==================== TABLE RENDEZ-VOUS ====================
CREATE TABLE RendezVous (
    IdRendezVous    INT IDENTITY(1,1) PRIMARY KEY,
    IdAnimal        INT NOT NULL REFERENCES Animaux(IdAnimal),
    IdVeterinaire   INT NOT NULL REFERENCES Utilisateurs(IdUtilisateur),
    DateHeure       DATETIME NOT NULL,
    Motif           NVARCHAR(255),
    Statut          NVARCHAR(20) NOT NULL DEFAULT 'Planifié'
                    CHECK (Statut IN ('Planifié','Confirmé','Annulé','Terminé')),
    Notes           NVARCHAR(500)
);
GO

-- ==================== TABLE CONSULTATIONS ==================
CREATE TABLE Consultations (
    IdConsultation  INT IDENTITY(1,1) PRIMARY KEY,
    IdRendezVous    INT REFERENCES RendezVous(IdRendezVous),
    IdAnimal        INT NOT NULL REFERENCES Animaux(IdAnimal),
    IdVeterinaire   INT NOT NULL REFERENCES Utilisateurs(IdUtilisateur),
    DateConsultation DATETIME NOT NULL DEFAULT GETDATE(),
    Diagnostic      NVARCHAR(MAX),
    Traitement      NVARCHAR(MAX),
    Remarques       NVARCHAR(MAX),
    Poids           DECIMAL(5,2),
    Temperature     DECIMAL(4,1)
);
GO

-- ==================== TABLE VACCINATIONS ===================
CREATE TABLE Vaccinations (
    IdVaccination   INT IDENTITY(1,1) PRIMARY KEY,
    IdAnimal        INT NOT NULL REFERENCES Animaux(IdAnimal),
    IdVeterinaire   INT NOT NULL REFERENCES Utilisateurs(IdUtilisateur),
    NomVaccin       NVARCHAR(100) NOT NULL,
    DateAdministration DATE NOT NULL,
    DateRappel      DATE,
    Lot             NVARCHAR(50),
    Notes           NVARCHAR(255)
);
GO

-- ==================== TABLE MÉDICAMENTS ====================
CREATE TABLE Medicaments (
    IdMedicament    INT IDENTITY(1,1) PRIMARY KEY,
    Nom             NVARCHAR(150) NOT NULL,
    Description     NVARCHAR(500),
    Unite           NVARCHAR(30)  NOT NULL DEFAULT 'comprimé',
    QuantiteStock   INT NOT NULL DEFAULT 0,
    StockMinimum    INT NOT NULL DEFAULT 10,
    PrixUnitaire    DECIMAL(10,2) NOT NULL,
    DateExpiration  DATE,
    Fournisseur     NVARCHAR(150)
);
GO

-- ========== TABLE CONSULTATION-MÉDICAMENT ===========
CREATE TABLE ConsultationMedicaments (
    IdConsultationMed INT IDENTITY(1,1) PRIMARY KEY,
    IdConsultation    INT NOT NULL REFERENCES Consultations(IdConsultation),
    IdMedicament      INT NOT NULL REFERENCES Medicaments(IdMedicament),
    Quantite          INT NOT NULL,
    PosologieDuree    NVARCHAR(200)
);
GO

-- ===================== TABLE FACTURES ======================
CREATE TABLE Factures (
    IdFacture           INT IDENTITY(1,1) PRIMARY KEY,
    IdConsultation      INT NOT NULL REFERENCES Consultations(IdConsultation),
    IdProprietaire      INT NOT NULL REFERENCES Proprietaires(IdProprietaire),
    DateFacture         DATETIME NOT NULL DEFAULT GETDATE(),
    MontantConsultation DECIMAL(10,2) NOT NULL DEFAULT 0,
    MontantMedicaments  DECIMAL(10,2) NOT NULL DEFAULT 0,
    MontantTotal        AS (MontantConsultation + MontantMedicaments) PERSISTED,
    Statut              NVARCHAR(20) NOT NULL DEFAULT 'Non payée'
                        CHECK (Statut IN ('Non payée','Payée','Partiellement payée')),
    ModePaiement        NVARCHAR(30),
    DatePaiement        DATETIME,
    Notes               NVARCHAR(255)
);
GO

-- =================== DONNÉES INITIALES =====================

-- admin / Admin123   → SHA256: 240be518fabd2724ddb6f04eeb1da5967448d7e831d06d456c490b25e2018bbd
INSERT INTO Utilisateurs (Nom, Prenom, Login, MotDePasse, Role)
VALUES ('Système', 'Admin', 'admin',
        '240be518fabd2724ddb6f04eeb1da5967448d7e831d06d456c490b25e2018bbd',
        'Administrateur');

-- secretaire / Secret123
INSERT INTO Utilisateurs (Nom, Prenom, Login, MotDePasse, Role)
VALUES ('Dupont', 'Marie', 'secretaire',
        '1b3a65ce3498a1e51da9ce413a2b6aef4e9a61db1e44b8df37f8b49b63c4b96a',
        'Secretaire');

-- vet / Vet12345
INSERT INTO Utilisateurs (Nom, Prenom, Login, MotDePasse, Role, Telephone)
VALUES ('Martin', 'Pierre', 'vet',
        'c72cb0e35a2e2ae9b14543c1cdcbc9b1fa8f1e46a5f4b1a9a46e4a8b3c7d2f10',
        'Veterinaire', '0600000001');

-- Médicaments de base
INSERT INTO Medicaments (Nom, Unite, QuantiteStock, StockMinimum, PrixUnitaire, DateExpiration)
VALUES
    ('Amoxicilline 250mg',      'comprimé', 200, 20, 0.50, '2026-12-31'),
    ('Métronidazole 500mg',     'comprimé', 150, 15, 0.80, '2026-09-30'),
    ('Meloxicam 1mg/ml',        'ml',       500, 50, 0.30, '2027-03-31'),
    ('Sérum physiologique 500ml','flacon',   30,  5, 4.50, '2027-06-30'),
    ('Vaccin Rage',             'dose',      50, 10,12.00, '2026-08-31'),
    ('Frontline Spot-On',       'pipette',   80, 10, 8.50, '2027-01-31');

-- Données de démonstration : propriétaires
INSERT INTO Proprietaires (Nom, Prenom, Adresse, Telephone, Email)
VALUES
    ('Ben Ali',   'Sami',   '12 rue de la Paix, Tunis',    '71000001', 'sami.benali@email.tn'),
    ('Trabelsi',  'Leila',  '5 avenue Bourguiba, Sfax',    '74000002', 'leila@email.tn'),
    ('Chaabane',  'Karim',  '8 rue Ibn Khaldoun, Sousse',  '73000003', 'karim@email.tn');

-- Animaux de démonstration
INSERT INTO Animaux (IdProprietaire, Nom, Espece, Race, Sexe, DateNaissance, Couleur)
VALUES
    (1, 'Rex',    'Chien', 'Berger Allemand', 'M', '2020-03-15', 'Fauve'),
    (1, 'Mimi',   'Chat',  'Siamois',         'F', '2021-07-01', 'Crème'),
    (2, 'Bouba',  'Chien', 'Labrador',        'M', '2019-11-20', 'Noir'),
    (3, 'Luna',   'Chat',  'Persan',          'F', '2022-01-10', 'Blanc');
GO

-- ====================== VUES ==============================

CREATE VIEW vw_PlanningVet AS
SELECT
    rv.IdRendezVous, rv.DateHeure, rv.Motif, rv.Statut,
    a.Nom AS NomAnimal, a.Espece,
    p.Nom + ' ' + p.Prenom AS Proprietaire,
    u.Nom + ' ' + u.Prenom AS Veterinaire,
    u.IdUtilisateur AS IdVet
FROM RendezVous rv
JOIN Animaux      a ON rv.IdAnimal      = a.IdAnimal
JOIN Proprietaires p ON a.IdProprietaire = p.IdProprietaire
JOIN Utilisateurs  u ON rv.IdVeterinaire = u.IdUtilisateur;
GO

CREATE VIEW vw_StockFaible AS
SELECT IdMedicament, Nom, QuantiteStock, StockMinimum, DateExpiration
FROM Medicaments
WHERE QuantiteStock <= StockMinimum
   OR DateExpiration <= DATEADD(DAY, 30, GETDATE());
GO

CREATE VIEW vw_DashboardStats AS
SELECT
    (SELECT COUNT(*) FROM RendezVous  WHERE CAST(DateHeure AS DATE) = CAST(GETDATE() AS DATE)) AS RdvAujourdhui,
    (SELECT COUNT(*) FROM RendezVous  WHERE MONTH(DateHeure)=MONTH(GETDATE()) AND YEAR(DateHeure)=YEAR(GETDATE())) AS RdvCeMois,
    (SELECT COUNT(*) FROM Consultations WHERE MONTH(DateConsultation)=MONTH(GETDATE()) AND YEAR(DateConsultation)=YEAR(GETDATE())) AS ConsultationsCeMois,
    (SELECT ISNULL(SUM(MontantTotal),0) FROM Factures WHERE MONTH(DateFacture)=MONTH(GETDATE()) AND YEAR(DateFacture)=YEAR(GETDATE())) AS RevenusCeMois,
    (SELECT COUNT(*) FROM Animaux WHERE Actif=1) AS TotalAnimaux,
    (SELECT COUNT(*) FROM Proprietaires) AS TotalProprietaires;
GO
