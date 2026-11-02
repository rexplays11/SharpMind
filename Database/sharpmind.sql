-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Nov 02, 2026 at 06:13 PM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.0.30

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `sharpmind`
--

-- --------------------------------------------------------

--
-- Table structure for table `korisnici`
--

CREATE TABLE `korisnici` (
  `korisnik_id` int(11) NOT NULL,
  `korisnicko_ime` varchar(20) NOT NULL,
  `lozinka` varchar(255) NOT NULL,
  `datum_registracije` date NOT NULL,
  `uloga_id` int(11) DEFAULT NULL,
  `nivoId` int(11) NOT NULL DEFAULT 1,
  `profesor_id` int(11) DEFAULT NULL,
  `ukupno_poena` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `korisnici`
--

INSERT INTO `korisnici` (`korisnik_id`, `korisnicko_ime`, `lozinka`, `datum_registracije`, `uloga_id`, `nivoId`, `profesor_id`, `ukupno_poena`) VALUES
(1, '12', '1', '2026-01-01', 1, 21, NULL, 1114),
(10, '1223', '', '2026-02-08', 1, 5, NULL, 0),
(15, 'awfaw13131', 'watawtawtaw', '2026-02-15', 1, 1, NULL, 0),
(18, 'wawfwafwa', 'PImldPWAWp', '2026-03-12', 3, 1, NULL, 0),
(26, 'awf', 'awffawafw', '2026-04-02', 1, 1, NULL, 0),
(27, 'faw', 'wfawfafaw', '2026-04-02', 1, 1, NULL, 4),
(28, '333333333333333', '333333333333333', '2026-04-02', 3, 3, NULL, 3),
(29, 'wafwfa333333', '33333333', '2026-04-02', 1, 1, 26, 4),
(46, 'Korisnik2', 'a1234', '2026-06-11', 2, 1, NULL, 0),
(47, '2', '3', '2026-06-25', 3, 2, NULL, 131),
(48, '3', '3', '2026-06-25', 2, 1, 48, 71),
(49, '66', 'vnbdVihUwm', '2026-06-26', 2, 1, 46, 22),
(68, '13', '1', '2026-06-26', 1, 1, 46, 4),
(75, '25', '25252', '2026-06-26', 1, 1, NULL, 52),
(95, 'wafwfafwaE', '242', '2026-06-26', 1, 1, 48, 0),
(100, 'Profesor', '12345', '2026-07-30', 2, 1, NULL, 249),
(103, 'Novi', '123', '2026-08-20', 2, 1, NULL, 0),
(104, 'Dabar', '133113', '2026-08-07', 2, 5, NULL, 41414444),
(105, '2ar2a', 'r2r2ara2', '2026-08-07', 2, 3, 75, 0),
(108, '12131341', '1414141', '2026-10-08', 2, 5, 47, 4224),
(110, '131', '1313131', '2026-11-10', 2, 2, 68, 13),
(111, '3131', '3131', '2026-11-02', 2, 2, 48, 11),
(113, 'wafaw', 'fawfwafaw', '2026-11-13', 2, 1, 49, 4242),
(116, '11111', 'IsyLupSEdZ', '2026-11-02', 2, 1, NULL, 11),
(117, '1234', '0JAmzuT#PH', '2026-11-02', 3, 1, NULL, 1),
(118, '131315', 'NdW#zoldZj', '2026-11-02', 2, 1, 113, 33),
(119, '1213141', '41414141', '2026-11-11', 1, 2, 95, 242),
(120, '131313131', 'fwafwa', '2026-11-11', 2, 3, 113, 251),
(121, '13114141', 'LIQI!xa1uW', '2026-11-02', 3, 1, NULL, 19),
(125, '5', '5', '2026-11-02', 3, 1, 48, 7),
(126, 'nikola2', '1234', '2026-11-02', 2, 1, NULL, 0);

-- --------------------------------------------------------

--
-- Table structure for table `kvizovi`
--

CREATE TABLE `kvizovi` (
  `kviz_id` int(11) NOT NULL,
  `kviz_naslov` varchar(255) DEFAULT NULL,
  `korisnik_id` int(11) DEFAULT NULL,
  `tip_kviza` varchar(50) DEFAULT NULL,
  `vremensko_ogranicenje` int(11) DEFAULT NULL,
  `broj_zivota` int(11) DEFAULT NULL,
  `datum_kreiranja` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `kvizovi`
--

INSERT INTO `kvizovi` (`kviz_id`, `kviz_naslov`, `korisnik_id`, `tip_kviza`, `vremensko_ogranicenje`, `broj_zivota`, `datum_kreiranja`) VALUES
(25, '63636363', 1, 'vreme', 120, NULL, '2026-07-10 20:48:01');

-- --------------------------------------------------------

--
-- Table structure for table `kviz_pitanja`
--

CREATE TABLE `kviz_pitanja` (
  `kviz_pitanje_id` int(11) NOT NULL,
  `kviz_id` int(11) NOT NULL,
  `pitanje_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `kviz_pitanja`
--

INSERT INTO `kviz_pitanja` (`kviz_pitanje_id`, `kviz_id`, `pitanje_id`) VALUES
(137, 25, 3),
(138, 25, 4),
(139, 25, 5),
(140, 25, 6),
(141, 25, 8),
(142, 25, 9);

-- --------------------------------------------------------

--
-- Table structure for table `lekcija`
--

CREATE TABLE `lekcija` (
  `lekcija_id` int(11) NOT NULL,
  `lekcija_naslov` varchar(100) NOT NULL,
  `lekcija_tekst` text NOT NULL,
  `poglavlje_id` int(11) DEFAULT NULL,
  `potpoglavlje_id` int(11) DEFAULT NULL,
  `nivo_id` int(11) NOT NULL,
  `korisnik_id` int(11) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `lekcija`
--

INSERT INTO `lekcija` (`lekcija_id`, `lekcija_naslov`, `lekcija_tekst`, `poglavlje_id`, `potpoglavlje_id`, `nivo_id`, `korisnik_id`) VALUES
(2, 'Šta je .NET Framework', '.NET (čita se „dot net”) je razvojna platforma koju je napravio Microsoft.\r\n.NET sadrži veliki skup biblioteka (gotovog koda) i alatki koje pomažu u pisanju i pokretanju programa.\r\nC# je glavni jezik koji se koristi na .NET platformi, ali podržava i druge jezike kao što su VB.NET, F# i drugi.', 1, 14, 1, 1),
(3, 'Kako izgleda C# program', 'Svaki C# program ima najmanje jednu klasu i jednu metodu pod nazivom Main().\r\n\r\nEvo kako izgleda jednostavan program u C#:\r\n\r\nusing System;\r\n\r\nclass Program\r\n{\r\n    static void Main()\r\n    {\r\n        Console.WriteLine(\"Zdravo, C#!\");\r\n    }\r\n}\r\n\r\nKada se pokrene ovaj program , na ekranu će se prikazati poruka Zdravo, C#!', 1, 11, 1, 1),
(4, 'Prednosti učenja C# jezika', 'Učenje C# jezika pruža mnoge prednosti:\r\n\r\n-Lakoća učenja za početnike.\r\n\r\n-Velika podrška zajednice i dokumentacije.\r\n\r\n-Mogućnost razvoja igara, aplikacija i web servisa.', 1, 5, 1, 1),
(5, 'Šta je .NET CLR?', 'CLR (Common Language Runtime) je glavni deo .NET platforme koji omogućava izvršavanje C# programa.', 1, 5, 1, 1),
(6, 'Šta je varijabla u C#?', 'Varijabla u C# je mesto u memoriji gde se čuva neka vrednost tokom izvršavanja programa.\r\nSvaka varijabla ima:\r\n\r\n-ime (kako je program prepoznaje),\r\n\r\n-tip podatka (koju vrstu vrednosti čuva, npr. broj, tekst, slovo),\r\n\r\n-vrednost (podatak koji se u njoj nalazi).', 1, 5, 1, 1),
(7, 'Šta su primitivni tipovi?', 'Primitivni tipovi su osnovni tipovi podataka u C#.\r\nKoriste se za čuvanje najjednostavnijih vrednosti kao što su brojevi, slova i logičke vrednosti.\r\nU C# primitivni tipovi uključuju cele brojeve, decimalne brojeve, logičke tipove i karaktere.\r\nVrednosti primitivnih tipova se čuvaju direktno u memoriji i ne smatraju se objektima (osim kada se konvertuju u objekte)', 1, 6, 1, 1),
(8, 'Šta je int?', 'int je najčešće korišćen tip za čuvanje celih brojeva u C#. Koristi se kada treba raditi sa brojanjem, indeksima ili matematičkim operacijama.\r\n\r\nPrimer:\r\n\r\nint godina = 2025;', 1, 6, 1, 1),
(9, 'Šta je double?', 'double služi za čuvanje decimalnih (realnih) brojeva sa dvostrukom preciznošću.\r\nOvo je najčešći tip za rad sa decimalama u C#.\r\n\r\nPrimer:\r\n\r\ndouble cena = 12.99;', 1, 6, 1, 1),
(10, 'Šta je float?', 'float je decimalni tip sa manjom preciznošću od double. Za float Vrednost mora se završiti slovom f.\r\n\r\nPrimer:\r\n\r\nfloat temperatura = 23.5f;', 1, 6, 1, 1),
(11, 'Šta je bool?', 'bool tip može da ima samo dve vrednosti:\r\n\r\n-true (tačno)\r\n\r\n-false (netačno)\r\n\r\nKoristi se u uslovima, proverama i grananju.\r\n\r\nPrimer:\r\n\r\nbool jeLogovan = true;', 1, 6, 1, 1),
(12, 'Šta je char?', 'char se koristi za čuvanje jednog znaka — slova, cifre ili simbola.\r\nMora biti naveden u jednostrukim navodnicima.\r\n\r\nPrimer:\r\n\r\nchar slovo = \'A\';', 1, 6, 1, 1),
(13, 'Šta je string?', 'string čuva niz karaktera — rečenice, reči ili bilo kakav tekst.\r\nZa razliku od char, string koristi dvostruke navodnike.\r\n\r\nPrimer:\r\n\r\nstring ime = \"Marko\";', 1, 6, 1, 1),
(14, 'Šta je objekat u C#?', 'Objekat u C# je instanca klase — konkretan primerak koji se kreira u memoriji na osnovu definicije klase.\r\n\r\nKlasa je šablon, dok je objekat stvarni „proizvod“ napravljen po tom šablonu.\r\n\r\nKljučne karakteristike objekta:\r\n\r\n-Sadrži podatke (polja, svojstva)\r\n\r\n-Ima ponašanje (metode)\r\n\r\n-Čuva stanje (trenutne vrednosti svojih polja)\r\n\r\n-Komunicira sa drugim objektima pozivanjem metoda', 1, 19, 1, 1),
(15, 'Kako se kreira objekat?', 'Za kreiranje objekta koristi se ključna reč new:\r\n\r\nCar mojAuto = new Car();\r\n\r\nOvde imamo:\r\n\r\nCar je klasa,\r\n\r\nmojAuto je objekat (instanca klase Car).\r\n\r\nPrimer klase i objekta\r\nclass Pas\r\n{\r\n    public string Ime;\r\n    public void Laje()\r\n    {\r\n        Console.WriteLine(Ime + \" laje!\");\r\n    }\r\n}\r\n\r\nPas p = new Pas();\r\np.Ime = \"Reks\";\r\np.Laje(); ', 1, 19, 1, 1),
(16, 'Šta je objektno-orijentisano programiranje (OOP)?', 'Objektno orijentisano programiranje (OOP) je model programiranja koji organizuje dizajn softvera oko podataka, odnosno objekata, umesto oko funkcija i logike. Objekat se može definisati kao podatkovno polje koje ima jedinstvene atribute i ponašanje.', 1, 19, 1, 1),
(17, 'Koncepti objektno orijentisanog programiranja (OOP)', 'Objektno orijentisano programiranje se zasniva na četiri osnovna koncepta:\r\n\r\n1.Enkapsulacija \r\n-Skrivanje unutrašnjeg stanja objekta i omogućavanje pristupa samo putem metoda.\r\nCilj je zaštita podataka i kontrolisati pristup.\r\n\r\n2.Nasleđivanje \r\n-Mogućnost da jedna klasa nasledi osobine i metode druge klase.\r\nOlakšava ponovnu upotrebu koda i izgradnju kompleksnijih struktura.\r\n\r\n3. Polimorfizam \r\n-Sposobnost da objekti različitih tipova odgovore na isti poziv metode na različite načine.\r\nOmogućava fleksibilnost i proširivost programa.\r\n\r\n4. Apstrakcija \r\n-Prikazivanje samo najvažnijih informacija, dok se detalji implementacije skrivaju.\r\nPomaže lakšem korišćenju kompleksnih sistema.', 1, 19, 1, 1),
(18, 'Šta je niz?', 'Niz je kolekcija elemenata istog tipa, koja se čuva u memoriji.\r\nSluži za čuvanje više vrednosti u jednoj promenljivoj.', 4, 11, 2, 1),
(19, 'Deklarisanje i inicijalizacija niza.', 'Deklaracija znači da kreiramo promenljivu tipa niz, ali joj još ne dodeljujemo prostor u memoriji.\r\n\r\nint[] brojevi;\r\n\r\nInicijalizacija znači dodeljivanje memorije i definisanje koliko elemenata niz ima.\r\n\r\nbrojevi = new int[5];\r\n\r\nSada niz ima 5 elemenata, sva 5 elemenata imaju vrednost 0.', 4, 11, 2, 1),
(20, 'Pristup elementima niza', 'Svakom elementu pristupamo preko indeksa, prvi element je indeks 0, drugi je 1 itd.\r\n\r\nKreiramo sledeći niz:\r\n\r\nint[] brojevi = { 10, 20, 30, 40, 50 };\r\n\r\nint prvi = brojevi[0];  // prvi element je 10\r\nint treci = brojevi[2]; // treći element je 30\r\n\r\nbrojevi[1] = 25;        // menjamo element sa indeks broj 1.', 4, 11, 2, 1),
(21, 'Switch naredba u C#', 'Switch je uslovna kontrolna struktura u C#, koja omogućava da se izvršava različit deo koda u zavisnosti od vrednosti neke promenljive.\r\nPredstavlja lakšu i pregledniju alternativu za više if - else if uslova.\r\n\r\nKoristi se najčešće kada imamo više mogućih izbora koji zavise od jedne promenljive.\r\n\r\nOsnovna struktura switch naredbe\r\nswitch (vrednost)\r\n{\r\n    case vrednost1:\r\n        // Kod koji se izvršava poklope vrednost i vrednost1\r\n        break;\r\n\r\n    case vrednost2:\r\n        // Kod koji se izvršava poklope vrednost i vrednost2\r\n        break;\r\n\r\n    default:\r\n        // Izvršava se ako nijedan slučaj nije pogođen\r\n        break;\r\n}\r\n\r\ncase -Moguća vrednost promenljive\r\nbreak -Zatvara trenutni blok i sprečava dalje izvršavanja\r\ndefault -Izvršava se ako nijedan vrednost se ne podudara)\r\n', 2, 14, 2, 1),
(22, 'Primer upotrebe switch-a.', 'int dan = 3;\r\n\r\nswitch (dan)\r\n{\r\n    case 1:\r\n        Console.WriteLine(\"Ponedeljak\");\r\n        break;\r\n    case 2:\r\n        Console.WriteLine(\"Utorak\");\r\n        break;\r\n    case 3:\r\n        Console.WriteLine(\"Sreda\");\r\n        break;\r\n    default:\r\n        Console.WriteLine(\"Nepoznat dan\");\r\n        break;\r\n}\r\n\r\n Rezultat će biti - Sreda.', 2, 14, 1, 1),
(24, 'if i else u C#', 'If i else su osnovne uslovne naredbe u C# koje omogućavaju programu da izvrši kod u zavisnosti tačnosti neke izjave.\r\n\r\nProgram proverava da li je uslov tačan (true) ili netačan (false) i na osnovu toga odlučuje koji deo koda će se izvršiti.\r\n\r\nif (uslov)\r\n{\r\n    // Kod se izvršava samo ako je uslov tačan (true)\r\n}\r\n\r\n- Else se koristi kada želimo da definišemo šta se dešava ako uslov NIJE tačan.\r\n\r\nif (uslov)\r\n{\r\n    // Izvršava se ako je uslov true\r\n}\r\nelse\r\n{\r\n    // Izvršava se ako je uslov false\r\n}\r\n\r\n- Ako postoji više mogućih odluka, koristi se else if.\r\n\r\nKoristimo if else kada nam je izbor binaran ili kad nam je potrebno granjanje.', 2, 13, 2, 1),
(25, 'Primer if else izjava.', 'int poeni = 75;\r\n\r\nif (poeni >= 90)\r\n{\r\n    Console.WriteLine(\"Ocena: 5\");\r\n}\r\nelse if (poeni >= 75)\r\n{\r\n    Console.WriteLine(\"Ocena: 4\");\r\n}\r\nelse if (poeni >= 60)\r\n{\r\n    Console.WriteLine(\"Ocena: 3\");\r\n}\r\nelse if (poeni >= 45)\r\n{\r\n    Console.WriteLine(\"Ocena: 2\");\r\n}\r\nelse\r\n{\r\n    Console.WriteLine(\"Ocena: 1\");\r\n}\r\n\r\nProgram proverava redom svaki if i else if uslov. Kada pronađe prvi tačan uslov (u ovom slučaju će pisati \"Ocena: 4\") — taj blok se izvršava i ostali se preskaču.\r\nAko nijedan uslov nije tačan, aktivira se else.', 2, 14, 2, 1),
(26, 'Petlje u C#.', 'Petlje u C# omogućavaju ponavljanje koda više puta.\r\nKoriste se kada želimo da se određena radnja izvrši više puta — npr. štampanje brojeva, prolazak kroz niz, unos podataka, sabiranje elemenata itd.\r\n\r\n', 2, 8, 2, 1),
(27, 'Vrste petlje.', '1. for petlja\r\n\r\nKoristi se kada unapred znamo koliko puta treba ponoviti kod.\r\n\r\nfor (int i = 1; i <= 5; i++)\r\n{\r\n    Console.WriteLine(\"Broj: \" + i);\r\n}\r\n\r\n- Ispisuje brojeve od 1 do 5.\r\n\r\n2. while petlja\r\n\r\nIzvršava se dok je uslov tačan (true).\r\n\r\nint i = 1;\r\n\r\nwhile (i <= 5)\r\n{\r\n    Console.WriteLine(\"Broj: \" + i);\r\n    i++;\r\n}\r\n\r\n3. do-while petlja\r\n\r\nKod se izvršava barem jednom, pa se uslov proverava nakon toga.\r\n\r\nint i = 1;\r\n\r\ndo\r\n{\r\n    Console.WriteLine(\"Broj: \" + i);\r\n    i++;\r\n}\r\nwhile (i <= 5);\r\n\r\n4. foreach petlja\r\n\r\nKoristi se za prolazak kroz nizove, liste i kolekcije.\r\n\r\nstring[] imena = { \"Ana\", \"Marko\", \"Jovan\" };\r\n\r\nforeach (string ime in imena)\r\n{\r\n    Console.WriteLine(ime);\r\n}\r\n\r\n- Ispisuje sve elemente iz niza imena.', 2, 8, 2, 1),
(28, 'Upravljanje izuzecima (Exceptions) u C#', 'Izuzeci su greške koje se događaju tokom izvršavanja programa.\r\nKada dođe do izuzetka, program se zaustavlja osim ako greška nije uhvaćena i pravilno obrađena.\r\n\r\nU C# izuzetke rešavamo pomoću try-catch-finally strukture.\r\n\r\n- try — blok u kome može doći do greške\r\n\r\nKod unutar try se izvršava normalno, ali ako dođe do greške, kontrola prelazi na catch.\r\n\r\n- catch — hvatanje greške\r\n\r\nOvaj blok se izvršava kada se desi izuzetak.\r\nMože uhvatiti određenu vrstu greške ili sve izuzetke.\r\n\r\n- finally (opciono)\r\n\r\nOvaj deo koda se izvršava uvek — bilo da je došlo do greške ili ne.', 15, 18, 2, 1),
(29, 'Primer upravljanje izuzecima.', 'try\r\n{\r\n    int broj = 10;\r\n    int rezultat = broj / 0; // Greška: deljenje nulom\r\n}\r\ncatch (Exception e)\r\n{\r\n    Console.WriteLine(\"Došlo je do greške: \" + e.Message);\r\n}\r\nfinally\r\n{\r\n    Console.WriteLine(\"Ovaj deo se izvršava uvek.\");\r\n}\r\n\r\nU ovom primeru ne samo što će biti ispisano deo koda koji se nalazi u finally, već će biti ispisana \"Došlo je do greške:\" sa DivideByZeroException izjavom.', 15, 18, 2, 1),
(30, 'Lista u C#', 'Lista je generička lista koja može da čuva elemente određenog tipa (int, string, objekti itd.).\r\nZa razliku od običnog niza (array), lista se može dinamički proširivati i smanjivati.\r\n\r\n-Deklaracija i dodavanje elemenata\r\nList<int> brojevi = new List<int>();\r\n\r\nbrojevi.Add(10);\r\nbrojevi.Add(20);\r\nbrojevi.Add(30);\r\n\r\n-Pristup elementima\r\n\r\nConsole.WriteLine(brojevi[0]); // 10\r\n\r\n-Uklanjanje elemenata\r\n\r\nbrojevi.Remove(20);    // uklanja vrednost 20\r\nbrojevi.RemoveAt(0);   // uklanja element na indeksu 0\r\n\r\nPrednosti liste su dinamična veličina, mnogo metoda, i lak pristup indeksu', 4, 11, 3, 1),
(31, 'ArrayList u C#', 'ArrayList je starija verzija liste koja može čuvati elemente različitih tipova.\r\n\r\nArrayList lista = new ArrayList();\r\n\r\nlista.Add(10);\r\nlista.Add(\"tekst\");\r\nlista.Add(3.14);\r\n\r\nMože doći do greške ako se ne obrati pažnja na tipove podataka.', 4, 11, 3, 1),
(32, 'Red (Queue) u C#.', 'Queue je kolekcija koja radi po principu FIFO — First In, First Out.\r\nPrvi ubačen element je prvi koji izlazi iz reda.\r\n\r\nPrimer:\r\n\r\nQueue<string> red = new Queue<string>();\r\n\r\nred.Enqueue(\"Marko\");\r\nred.Enqueue(\"Ana\");\r\nred.Enqueue(\"Jovan\");\r\n\r\n-Izbacivanje sa reda:\r\n\r\nConsole.WriteLine(red.Dequeue()); // Marko izlazi prvi\r\n\r\n-Pogled sledećeg u redu bez uklanjanja\r\nConsole.WriteLine(red.Peek()); // Ana\r\n', 4, 9, 3, 1),
(33, 'Stek u C#.', 'Stack radi po principu LIFO — Last In, First Out.\r\nPoslednji ubačen element je prvi koji izlazi.\r\n\r\nPrimer:\r\nStack<int> stek = new Stack<int>();\r\n\r\nstek.Push(1);\r\nstek.Push(2);\r\nstek.Push(3);\r\n\r\n-Uzimanje sa vrha\r\nConsole.WriteLine(stek.Pop()); // 3 izlazi prvi\r\n\r\n-Pogled bez uklanjanja\r\nConsole.WriteLine(stek.Peek()); // 2\r\n\r\n\r\n-Koristi se za algoritme, pozivni stek funkcija, undo sistemi itd.', 4, 20, 3, 1),
(34, 'LinkedList u C#.', 'LinkedLista je lista elemenata povezanih čvorovima.\r\nZa razliku od Liste, elementi se ne nalaze u memoriji redom, već svaki sadrži pokazivač na sledeći i prethodni.\r\n\r\nPrimer:\r\n\r\nLinkedList<string> imena = new LinkedList<string>();\r\n\r\nimena.AddLast(\"Ana\");\r\nimena.AddLast(\"Marko\");\r\nimena.AddFirst(\"Jovan\");\r\n\r\n\r\n-Redosled sada: Jovan - Ana - Marko\r\n\r\n-Uklanjanje\r\n\r\nimena.Remove(\"Ana\");\r\nimena.RemoveFirst();\r\nimena.RemoveLast();\r\n\r\nPrednosti su brzo ubacivanje i brisanje.', 4, 20, 3, 1),
(35, 'Matrice u C# (Multidimenzionalni nizovi)', 'Matrica je niz koji sadrži elemente raspoređene u više dimenzija (najčešće 2D).\r\nMožemo je zamisliti kao tabelu sa redovima i kolonama.\r\n\r\n-Deklaracija matrice:\r\n\r\nint[,] matrica;  // 2D matrica celih brojeva\r\n\r\n-Inicijalizacija matrice:\r\n\r\nint[,] matrica = new int[3,3]; // matrica 3x3\r\n\r\n-Možemo je odmah popuniti vrednostima:\r\n\r\nint[,] matrica = \r\n{\r\n    {1, 2, 3},\r\n    {4, 5, 6},\r\n    {7, 8, 9}\r\n};\r\n\r\n-Pristup elementima matrice:\r\n\r\nElementima pristupamo preko indeksa [red, kolona]:\r\n\r\nConsole.WriteLine(matrica[0,1]); // ispisuje 2\r\nConsole.WriteLine(matrica[2,2]); // ispisuje 9\r\n\r\nindeksiranje počinje od 0.\r\n\r\n-Promena vrednosti u matrici:\r\n\r\nmatrica[1,1] = 50; // menja 5 u 50\r\n\r\n-Petlja kroz matricu:\r\n\r\nNajčešće se koriste dve for petlje, jedna za redove, druga za kolone:\r\n\r\nfor(int i = 0; i < 3; i++)\r\n{\r\n    for(int j = 0; j < 3; j++)\r\n    {\r\n        Console.Write(matrica[i,j] + \" \");\r\n    }\r\n    Console.WriteLine();\r\n}', 4, 11, 3, 1);

-- --------------------------------------------------------

--
-- Table structure for table `nivoi`
--

CREATE TABLE `nivoi` (
  `NivoId` int(11) NOT NULL,
  `NazivNivoa` text NOT NULL,
  `granicni_poeni` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `nivoi`
--

INSERT INTO `nivoi` (`NivoId`, `NazivNivoa`, `granicni_poeni`) VALUES
(1, 'Početnik', 100),
(2, 'Srednji', 250),
(3, 'Napredni', 400),
(4, 'Ekspert', 750),
(5, 'Master', 1000),
(21, 'Super Master2', 1114);

-- --------------------------------------------------------

--
-- Table structure for table `odgovori`
--

CREATE TABLE `odgovori` (
  `odgovor_id` int(11) NOT NULL,
  `pitanje_id` int(11) DEFAULT NULL,
  `odgovor_tekst` text NOT NULL,
  `is_tacno` tinyint(1) DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `odgovori`
--

INSERT INTO `odgovori` (`odgovor_id`, `pitanje_id`, `odgovor_tekst`, `is_tacno`) VALUES
(1, 1, 'Proceduralni', 0),
(2, 1, 'Objektno-orijentisani', 1),
(3, 1, 'Funkcionalni', 0),
(4, 1, 'Skriptni', 0),
(5, 2, 'Interpretera', 0),
(6, 2, 'JVM-a', 0),
(7, 2, '.NET CLR', 1),
(8, 2, 'Python interpretera', 0),
(9, 3, 'Specijalna funkcija koja izvršava kod', 0),
(10, 3, 'Klasa koja nasleđuje druge klase', 0),
(11, 3, 'Kontejner za čuvanje vrednosti tokom izvršavanja programa', 1),
(12, 3, 'Vrsta petlje', 0),
(13, 4, 'Tip podataka koji čuva samo tekstualne vrednosti', 0),
(14, 4, 'Tip podataka koji direktno čuva vrednost u memoriji', 1),
(15, 4, 'Tip podataka koji mora biti objekat', 0),
(16, 4, 'Tip podataka koji ne može da se menja', 0),
(17, 5, 'int', 0),
(18, 5, 'bool', 0),
(19, 5, 'double', 1),
(20, 5, 'char', 0),
(21, 6, 'bool', 1),
(22, 6, 'int', 0),
(23, 6, 'char', 0),
(24, 6, 'string', 0),
(25, 7, 'Tačno', 1),
(26, 7, 'Netačno', 0),
(27, 8, 'string', 1),
(28, 8, 'char', 0),
(29, 8, 'int', 0),
(30, 8, 'bool', 0),
(31, 9, 'Tačno', 0),
(32, 9, 'Netačno', 1),
(33, 10, 'Promenljiva koja čuva samo tekstualne vrednosti', 0),
(34, 10, 'Instanca klase koja može da čuva podatke i koristi metode', 1),
(35, 10, 'Funkcija koja vraća rezultat', 0),
(36, 10, 'Tip podatka koji se ne može promeniti', 0),
(37, 11, 'Promenljivu koja ima vrednost 0', 0),
(38, 11, 'Promenljivu koja nije inicijalizovana ili ne pokazuje ni na šta', 1),
(39, 11, 'Promenljivu koja sadrži praznu listu', 0),
(40, 11, 'Promenljivu koja sadrži znak \'0\'', 0),
(41, 12, 'int broj;', 0),
(42, 12, 'int broj = 10;', 0),
(43, 12, 'var broj = 10;', 0),
(44, 12, 'Sve navedeno', 1),
(45, 13, 'Programiranje koje se bazira samo na funkcijama i procedurama', 0),
(46, 13, 'Programiranje koje koristi objekte i klase za organizaciju podataka i funkcionalnosti', 1),
(47, 13, 'Programiranje koje ne koristi promenljive', 0),
(48, 13, 'Programiranje koje se koristi samo za pravljenje mobilnih aplikacija', 0),
(49, 15, 'Nasleđivanje (Inheritance)', 0),
(50, 15, 'Polimorfizam (Polymorphism)', 0),
(51, 15, 'Enkapsulacija (Encapsulation)', 0),
(52, 15, 'Proceduralno grananje', 1),
(53, 16, 'Console.WriteLine(brojevi);', 0),
(54, 16, 'Console.WriteLine(brojevi[i]);', 1),
(55, 16, 'Console.WriteLine(i);', 0),
(56, 16, 'Console.WriteLine(brojevi.Length);', 0),
(57, 17, 'brojevi.Add(10);', 1),
(58, 17, 'brojevi.Insert(10);', 0),
(59, 17, 'brojevi[0] = 10;', 0),
(60, 17, 'brojevi.Push(10);', 0),
(61, 18, 'broj > 5', 1),
(62, 18, 'broj < 5', 0),
(63, 18, 'broj == 5', 0),
(64, 18, 'broj >= 5', 0),
(65, 19, 'for (int i = 0; i < brojevi.Length; i++)', 0),
(66, 19, 'foreach (int b in brojevi)', 1),
(67, 19, 'while (int b in brojevi)', 0),
(68, 19, 'do (int b in brojevi)', 0),
(69, 20, 'Nasleđivanje', 1),
(70, 20, 'Polimorfizam', 1),
(71, 20, 'Petlje', 0),
(72, 20, 'Apstrakcija', 1),
(73, 21, 'Kolekcija istovetnih tipova podataka koji se čuvaju u uzastopnim memorijskim lokacijama', 1),
(74, 21, 'Tip podataka koji može čuvati samo jedan element', 0),
(75, 21, 'Referenca na objekat koji može da menja vrednosti elemenata', 0),
(76, 21, 'Specijalna funkcija za izvršavanje koda', 0),
(77, 22, 'Promenljiva koja čuva više vrednosti', 0),
(78, 22, 'Naredba za izlazak iz programa', 0),
(79, 22, 'Blok koda koji izvršava određeni zadatak i može da se pozove više puta', 1),
(80, 22, 'Tip podataka za čuvanje teksta', 0),
(81, 23, 'Metoda uvek mora da vrati neku vrednost', 0),
(82, 23, 'Metoda koja ne vraća vrednost koristi ključnu reč void', 1),
(83, 23, 'Metoda ne može imati parametre', 0),
(84, 23, 'Metoda se ne može pozvati iz druge metode', 0),
(85, 24, 'Samo ako se nalazi u Main() metodi', 0),
(86, 24, 'Samo ako je tipa void', 0),
(87, 24, 'Kada je definisana i ima odgovarajuću vidljivost (npr. public)', 1),
(88, 24, 'Nikada, metode se izvršavaju automatski', 0),
(89, 25, 'exit', 0),
(90, 25, 'break', 0),
(91, 25, 'return', 1),
(92, 25, 'output', 0),
(93, 26, 'Da poziva samu sebe unutar svog tela', 1),
(94, 26, 'Da poziva više različitih metoda', 0),
(95, 26, 'Da se izvršava samo jednom', 0),
(96, 26, 'Da koristi više return naredbi', 0),
(97, 27, 'Da bi se izašlo iz trenutnog case bloka i sprečilo prelazak u sledeći', 1),
(98, 27, 'Da bi se završio program', 0),
(99, 27, 'Da bi se prešlo na sledeći case automatski', 0),
(100, 27, 'Da bi se ponovio isti blok više puta', 0),
(101, 28, 'Izvršava se ako nijedan case ne odgovara vrednosti izraza', 1),
(102, 28, 'Uvek se izvršava prvi', 0),
(103, 28, 'Označava kraj switch bloka', 0),
(104, 28, 'Izvršava se ako svi case odgovaraju', 0),
(105, 29, 'Promenljivu koja čuva tekstualne vrednosti', 0),
(106, 29, 'Šablon (model) prema kojem se kreiraju objekti', 1),
(107, 29, 'Naredbu za uslovno grananje', 0),
(108, 29, 'Tip podataka koji se koristi samo za nasleđivanje', 0),
(109, 30, 'Implementacija', 0),
(110, 30, 'Deklaracija', 0),
(111, 30, 'Instanciranje', 1),
(112, 30, 'Nasleđivanje', 0),
(113, 31, 'To je metoda koja se automatski poziva kada se kreira objekat', 1),
(114, 31, 'onstruktor mora imati povratni tip void', 0),
(115, 31, 'Konstruktor se ne može preopteretiti', 0),
(116, 31, 'Klasa ne može postojati bez konstruktora', 0),
(117, 32, 'Ispisuje \"Veliki broj\"', 0),
(118, 32, 'Ispisuje \"Mali broj\"', 1),
(119, 32, 'Program ne može da se pokrene', 0),
(120, 32, 'Ispisuje obe poruke', 0),
(121, 33, 'Ponedeljak', 0),
(122, 33, 'Utorak', 1),
(123, 33, 'Nepoznat dan', 0),
(124, 33, 'Greška pri izvršavanju', 0),
(125, 34, '1 put', 0),
(126, 34, '2 puta', 0),
(127, 34, '3 puta', 1),
(128, 34, '4 puta', 0),
(129, 35, 'Greška – metoda mora imati parametre', 0),
(130, 35, 'Ispisuje \"Zdravo, ja sam metoda!\"', 1),
(131, 35, 'Program se ne pokreće', 0),
(132, 35, 'Ispisuje praznu liniju', 0),
(133, 36, 'Greška pri pokretanju programa', 0),
(134, 36, 'Program ne ispisuje ništa', 0),
(135, 36, 'Ispisuje \"Kreirana nova osoba!\"', 1),
(136, 36, 'Ispisuje nasumičan tekst', 0),
(137, 37, 'Greška – konstruktor ne može imati parametre', 0),
(138, 37, 'Ispisuje \"Marka automobila je: BMW\"', 1),
(139, 37, 'Ispisuje \"BMW\"', 0),
(140, 37, 'Ne ispisuje ništa', 0),
(141, 38, 'Da se program odmah zaustavi kada dođe do greške', 0),
(142, 38, 'Da se spreči izvršavanje koda', 0),
(143, 38, 'Da se greške uhvate i program nastavi bez rušenja', 1),
(144, 38, 'Da se brišu podaci iz memorije', 0),
(145, 39, 'if, else, return', 0),
(146, 39, 'try, catch, finally', 1),
(147, 39, 'throw, continue, break', 0),
(148, 39, 'check, error, resume', 0),
(149, 40, 'throw', 1),
(150, 40, 'catch', 0),
(151, 40, 'raise', 0),
(152, 40, 'error', 0),
(153, 41, 'Izvršava se samo ako nema greške', 0),
(154, 41, 'Preskače se ako se dogodi izuzetak', 0),
(155, 41, 'Garantuje da će određen kod biti izvršen, bez obzira da li se desila greška ili ne', 1),
(156, 41, 'Briše grešku iz memorije', 0),
(157, 42, 'Greška pri pokretanju programa', 0),
(158, 42, 'Program ne ispisuje ništa', 0),
(159, 42, 'Ispisuje “Deljenje nulom nije dozvoljeno!”', 1),
(160, 42, 'Ispisuje “0”', 0),
(161, 43, 'Greška u sistemu!', 1),
(162, 43, 'Exception: Greška u sistemu!', 0),
(163, 43, 'Ništa', 0),
(164, 43, 'Greška pri kompajliranju', 0),
(165, 44, 'Tačno', 1),
(166, 44, 'Netačno', 0),
(167, 45, 'Tačno', 0),
(168, 45, 'Netačno', 1),
(169, 46, 'Tačno', 1),
(170, 46, 'Netačno', 0),
(171, 47, 'Tačno', 1),
(172, 47, 'Netačno', 0),
(173, 48, '2', 0),
(174, 48, '3', 1),
(175, 48, '4', 0),
(176, 48, '5', 0),
(177, 49, 'Program će ispisati \"tekst\"', 0),
(178, 49, 'Program će baciti grešku pri izvršavanju', 1),
(179, 49, 'Kompajler neće prihvatiti kod', 0),
(180, 49, 'broj će imati vrednost 0', 0),
(181, 50, 'A', 0),
(182, 50, 'B', 0),
(183, 50, 'C', 1),
(184, 50, 'Greška', 0),
(185, 51, '1', 1),
(186, 51, '2', 0),
(187, 51, '3', 0),
(188, 51, 'Greška', 0),
(189, 52, '5', 1),
(190, 52, '10', 0),
(191, 52, '20', 0),
(192, 52, 'Greška', 0),
(193, 53, 'Može menjati veličinu tokom izvršavanja', 1),
(194, 53, 'Omogućava pristup elementima po indeksu', 1),
(195, 53, 'Ne podržava tip object', 0),
(196, 53, 'Ne podržava tip String', 0),
(197, 54, 'Push()', 1),
(198, 54, 'Pop()', 1),
(199, 54, 'Peek()', 1),
(200, 54, 'Add()', 0),
(201, 55, 'Tačno', 0),
(202, 55, 'Netačno', 1),
(203, 56, 'Tačno', 1),
(204, 56, 'Netačno', 0),
(205, 57, 'Tačno', 0),
(206, 57, 'Netačno', 1),
(207, 58, 'Tačno', 1),
(208, 58, 'Netačno', 0),
(209, 59, 'Tačno', 1),
(210, 59, 'Netačno', 0),
(211, 60, '2', 0),
(212, 60, '3', 0),
(213, 60, '5', 0),
(214, 60, '6', 1),
(215, 61, '2', 0),
(216, 61, '3', 0),
(217, 61, '5', 0),
(218, 61, '6', 1),
(219, 62, '2', 1),
(220, 62, '5', 0),
(221, 62, '1', 0),
(222, 62, '0', 0),
(223, 63, 'Indeksi počinju od nule.', 1),
(224, 63, 'Mogu imati više od dve dimenzije (npr. trodimenzionalni niz).', 1),
(225, 63, 'Elementima se pristupa pomoću jedne zagrade, npr. niz[i][j].', 0),
(226, 63, 'Elementima se pristupa pomoću zapete, npr. niz[i, j].', 1),
(227, 64, 'Svi redovi moraju imati isti broj kolona.', 1),
(228, 64, 'Elementima se pristupa po indeksima reda i kolone.', 1),
(229, 64, 'Matrice mogu imati različit broj kolona po redu.', 0),
(230, 64, 'Matrica se može inicijalizovati pomoću ugneždenih viticastih zagrada.', 1),
(231, 65, 'Tačno', 1),
(232, 65, 'Netačno', 0),
(233, 66, 'Tačno', 1),
(234, 66, 'Netačno', 0),
(235, 67, 'Tačno', 1),
(236, 67, 'Netačno', 0),
(237, 68, 'Substring()', 1),
(238, 68, 'Add()', 0),
(239, 68, 'Replace()', 1),
(240, 68, 'Push()', 0),
(241, 69, 'Program', 1),
(242, 69, 'ing', 0),
(243, 69, 'ramira', 0),
(244, 69, 'Programi', 0),
(245, 70, 'Dobro jutro', 0),
(246, 70, 'Dobro veče', 1),
(247, 70, 'Dobro', 0),
(248, 70, 'veče', 0),
(249, 71, 'C', 0),
(250, 71, 'S', 0),
(251, 71, 'h', 1),
(252, 71, 'a', 0),
(253, 72, 'Tačno', 1),
(254, 72, 'Netačno', 0),
(255, 73, 'Tačno', 1),
(256, 73, 'Netačno', 0),
(257, 74, 'Tačno', 1),
(258, 74, 'Netačno', 0),
(259, 75, 'Metoda se poziva sama', 1),
(260, 75, 'Metoda ne poziva samu sebe', 0),
(261, 75, 'Ne može vraćati vrednost', 0),
(262, 75, 'Može biti kombinovana sa petljama', 1),
(263, 76, '15', 0),
(264, 76, '120', 1),
(265, 76, '25', 0),
(266, 76, '10', 0),
(267, 77, '4', 0),
(268, 77, '10', 1),
(269, 77, '0', 0),
(270, 77, '24', 0),
(271, 78, 'A', 0),
(272, 78, 'ABC', 1),
(273, 78, 'CBA', 0),
(274, 78, 'B', 0),
(275, 79, 'Tačno', 1),
(276, 79, 'Netačno', 0),
(277, 80, 'Tačno', 1),
(278, 80, 'Netačno', 0),
(279, 81, 'Tačno', 0),
(280, 81, 'Netačno', 1),
(281, 82, 'Hello', 0),
(282, 82, 'World', 0),
(283, 82, 'Hello World', 1),
(284, 82, 'test.txt', 0),
(285, 83, 'Fajl postoji', 0),
(286, 83, 'Fajl ne postoji', 1),
(287, 84, 'Tip promenljive kod kompajliranja', 1),
(288, 84, 'Uvek string', 0),
(289, 84, 'Nasleđivanje', 0),
(290, 84, 'Tip koji se određuje tek u runtime-u', 0),
(291, 85, 'Tačno', 1),
(292, 85, 'Netačno', 0),
(293, 86, 'Brže izvršavanje aplikacije', 0),
(294, 86, 'Bezbednost tipov', 1),
(295, 86, 'Lepši izgled koda', 0),
(296, 86, 'Manje korišćenje memorije', 0),
(297, 87, '10 Tekst', 1),
(298, 87, 'Tekst', 0),
(299, 87, '10', 0),
(300, 87, 'Greška u generičkom tipu', 0),
(301, 88, 'Jezik za pisanje web aplikacija', 1),
(302, 88, 'Tehnologija za upite nad podacima u C#', 0),
(303, 88, 'Sistem za upravljanje memorijom', 0),
(304, 88, 'Ugrađeni SQL server u C#', 0),
(305, 89, 'where', 1),
(306, 89, 'select', 1),
(307, 89, 'return', 1),
(308, 89, 'orderby', 0),
(309, 90, 'Tačno', 0),
(310, 90, 'Netačno', 1),
(311, 91, 'Tačno', 1),
(312, 91, 'Netačno', 0),
(313, 92, '4 5 6', 1),
(314, 92, '1 2 3', 0),
(315, 92, '6 5 4', 0),
(316, 92, '3 4', 0),
(317, 93, 'OrderBy()', 1),
(318, 93, 'Sort()', 0),
(319, 93, 'GroupBy()', 0),
(320, 93, 'Split()', 0);

-- --------------------------------------------------------

--
-- Table structure for table `pitanja`
--

CREATE TABLE `pitanja` (
  `pitanje_id` int(11) NOT NULL,
  `pitanje_tekst` text NOT NULL,
  `pitanje_tip` enum('VISESTRUKI_IZBOR','KOD','UNOS') NOT NULL,
  `nivo_id` int(11) DEFAULT NULL,
  `blok_koda` text DEFAULT NULL,
  `korisnik_id` int(11) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `pitanja`
--

INSERT INTO `pitanja` (`pitanje_id`, `pitanje_tekst`, `pitanje_tip`, `nivo_id`, `blok_koda`, `korisnik_id`) VALUES
(1, 'C# je ova vrsta programskog jezika', 'VISESTRUKI_IZBOR', 1, 'using System;\r\n\r\nnamespace HelloWorld\r\n{\r\n  class Program\r\n  {\r\n    static void Main(string[] args)\r\n    {\r\n      Console.WriteLine(\"\");    \r\n    }\r\n  }\r\n}', 1),
(2, 'C# se izvršava pomoću:', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(3, 'Šta je varijabla u C#', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(4, 'Šta je primitivni tip podataka u C#?', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(5, 'Koji tip podataka u C# se koristi za decimalne brojeve?', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(6, 'Koji tip podatka u C# čuva logičke vrednosti (true/false)', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(7, 'Ako se decimalni broj 7,45 konvertuje u int u C#, vrednost decimalnog dela se gubi.', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(8, 'Koji tip podatka se koristi za čuvanje teksta ili više karaktera u C#?', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(9, 'char promenljiva može da čuva više karaktera.', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(10, 'Šta je objekat u C#?', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(11, 'Šta predstavlja vrednost null u C#', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(12, 'Kako se pravilno deklarise promenljiva tipa int u C#?', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(13, 'Šta je objektno-orijentisano programiranje (OOP)?', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(15, 'Koji od navedenih nije koncept objektno-orijentisanog programiranja (OOP)?', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(16, 'Dovrši kod da bi ispisao sve elemente niza brojevi:\r\n\r\nint[] brojevi = {1, 2, 3, 4, 5};\r\nfor (int i = 0; i < brojevi.Length; i++)\r\n{\r\n    // Dovrši ovde\r\n}', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(17, 'Koji kod dodaje broj 10 u listu brojevi?\r\n\r\nList<int> brojevi = new List<int>();\r\n// Dodaj broj 10 u listu', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(18, 'Dovrši kod da proveri da li je broj veći od 5:\r\n\r\nint broj = 7;\r\nif (__________)\r\n{\r\n    Console.WriteLine(\"Veći od 5\");\r\n}\r\nelse\r\n{\r\n    Console.WriteLine(\"Manji ili jednak 5\");\r\n}', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(19, 'Koji kod ispravno prolazi kroz sve elemente niza brojevi i ispisuje ih?\r\n\r\nint[] brojevi = {1, 2, 3, 4, 5};\r\n___________\r\n{\r\n    Console.WriteLine(b);\r\n}', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(20, 'Koji od navedenih su osnovni koncepti objektno-orijentisanog programiranja (OOP)?', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(21, 'Šta je array u C#?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(22, 'Šta je metoda u C#?', 'VISESTRUKI_IZBOR', 1, NULL, 1),
(23, 'Koja od navedenih tvrdnji o metodama u C# je tačna?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(24, 'Kada se metoda može pozvati u programu?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(25, 'Koja ključna reč se koristi u metodi da bi se vratila vrednost?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(26, 'Šta znači da je metoda rekurzivna?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(27, 'Zašto se u switch bloku obično koristi break?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(28, 'Šta radi default deo u switch naredbi?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(29, 'Šta predstavlja klasa u C#?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(30, 'Kako se zove proces pravljenja objekta na osnovu klase?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(31, 'Koja stavka o konstruktoru klase je tačna?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(32, 'Šta će ispisati sledeći kod?\r\n\r\nint broj = 8;\r\nif (broj > 10)\r\n{\r\n    Console.WriteLine(\"Veliki broj\");\r\n}\r\nelse\r\n{\r\n    Console.WriteLine(\"Mali broj\");\r\n}', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(33, 'Šta će ispisati sledeći kod?\r\n\r\nint dan = 2;\r\nswitch (dan)\r\n{\r\n    case 1:\r\n        Console.WriteLine(\"Ponedeljak\");\r\n        break;\r\n    case 2:\r\n        Console.WriteLine(\"Utorak\");\r\n        break;\r\n    default:\r\n        Console.WriteLine(\"Nepoznat dan\");\r\n        break;\r\n}', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(34, 'Koliko puta će se ispisati poruka \"Zdravo\"?\r\n\r\nfor (int i = 0; i < 3; i++)\r\n{\r\n    Console.WriteLine(\"Zdravo\");\r\n}', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(35, 'Šta će ispisati sledeći kod?\r\n\r\nvoid Pozdrav()\r\n{\r\n    Console.WriteLine(\"Zdravo, ja sam metoda!\");\r\n}\r\n\r\nPozdrav();', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(36, 'Šta će ispisati sledeći kod?\r\n\r\nclass Osoba\r\n{\r\n    public Osoba()\r\n    {\r\n        Console.WriteLine(\"Kreirana nova osoba!\");\r\n    }\r\n}\r\n\r\nclass Program\r\n{\r\n    static void Main()\r\n    {\r\n        Osoba o = new Osoba();\r\n    }\r\n}', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(37, 'Šta će ispisati sledeći kod?\r\n\r\nclass Auto\r\n{\r\n    public Auto(string marka)\r\n    {\r\n        Console.WriteLine(\"Marka automobila je: \" + marka);\r\n    }\r\n}\r\n\r\nclass Program\r\n{\r\n    static void Main()\r\n    {\r\n        Auto a = new Auto(\"BMW\");\r\n    }\r\n}', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(38, 'Čemu služi upravljanje izuzecima u C#?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(39, 'Kojim ključnim rečima se u C# koristi upravljanje izuzecima?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(40, 'Kojom ključnom rečju se baca izuzetak u C#?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(41, 'Šta radi blok finally?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(42, 'Šta će ispisati sledeći kod?\r\n\r\ntry\r\n{\r\n    int x = 5;\r\n    int y = 0;\r\n    Console.WriteLine(x / y);\r\n}\r\ncatch (DivideByZeroException)\r\n{\r\n    Console.WriteLine(\"Deljenje nulom nije dozvoljeno!\");\r\n}', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(43, 'Šta će ispisati sledeći kod?\r\n\r\ntry\r\n{\r\n    throw new Exception(\"Greška u sistemu!\");\r\n}\r\ncatch (Exception e)\r\n{\r\n    Console.WriteLine(e.Message);\r\n}', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(44, 'if i switch služe za grananje toka programa.', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(45, 'U C# svaka klasa mora imati konstruktor.', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(46, 'U switch izrazu svaka case grana mora završiti sa break naredbom.', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(47, 'Metode u C# mogu vraćati vrednost pomoću return.', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(48, 'Šta će ispisati sledeći kod?\r\n\r\nList<int> brojevi = new List<int>() { 1, 2, 3 };\r\nbrojevi.Add(4);\r\nbrojevi.Remove(2);\r\nConsole.WriteLine(brojevi.Count);', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(49, 'Šta će se desiti kada se pokrene sledeći kod?\r\n\r\nArrayList lista = new ArrayList();\r\nlista.Add(\"tekst\");\r\nint broj = (int)lista[0];', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(50, 'Šta će ispisati sledeći kod?\r\n\r\nStack<string> stack = new Stack<string>();\r\nstack.Push(\"A\");\r\nstack.Push(\"B\");\r\nstack.Push(\"C\");\r\nConsole.WriteLine(stack.Pop());', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(51, 'Šta će ispisati sledeći kod?\r\n\r\nQueue<int> red = new Queue<int>();\r\nred.Enqueue(1);\r\nred.Enqueue(2);\r\nred.Enqueue(3);\r\nConsole.WriteLine(red.Dequeue());', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(52, 'Šta će ispisati sledeći kod?\r\n\r\nLinkedList<int> brojevi = new LinkedList<int>();\r\nbrojevi.AddLast(10);\r\nbrojevi.AddLast(20);\r\nbrojevi.AddFirst(5);\r\nConsole.WriteLine(brojevi.First.Value);', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(53, 'Koje karakteristike važe za List<T> u C#?', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(54, 'Koje metode ima Stack<T>?', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(55, 'ArrayList i List<T> su isto, samo različitog naziva.', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(56, 'LinkedList<T> omogućava brzo umetanje elemenata na početak i kraj.', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(57, 'U Queue<T> poslednji ubačeni element se prvi uklanja.', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(58, 'Metoda Peek() kod Stack<T> vraća gornji element bez njegovog uklanjanja.', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(59, 'Metoda Contains() postoji i u List<T> i u LinkedList<T>.', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(60, 'Šta će ispisati sledeći kod?\r\n\r\nint[,] matrica = {\r\n    { 1, 2, 3 },\r\n    { 4, 5, 6 }\r\n};\r\nConsole.WriteLine(matrica[1, 2]);', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(61, 'Šta će ispisati sledeći kod?\r\n\r\nint[,] brojevi = new int[2, 3];\r\nConsole.WriteLine(brojevi.Length);', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(62, 'Šta će ispisati sledeći kod?\r\n\r\nint[,] niz = { { 5, 10 }, { 15, 20 } };\r\nConsole.WriteLine(niz.GetLength(0));', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(63, 'Koje od navedenih stavki važe za višedimenzionalne nizove u C#?', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(64, 'Koje su tačne izjave o matricama (int[,])?', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(65, 'Višedimenzionalni nizovi u C# mogu imati više od dve dimenzije.', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(66, 'String u C# je tip podatka koji može sadržati više karaktera.', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(67, 'Metoda Length vraća broj karaktera u stringu.', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(68, 'Koja od sledećih metoda pripada stringovima u C#?', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(69, 'Šta će ispisati sledeći kod?\r\n\r\nstring rec = \"Programiranje\";\r\nConsole.WriteLine(rec.Substring(0, 7));', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(70, 'Šta će ispisati sledeći kod?\r\n\r\nstring rec = \"Dobro jutro\";\r\nConsole.WriteLine(rec.Replace(\"jutro\", \"veče\"));', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(71, 'Šta će ispisati sledeći kod?\r\n\r\nstring rec = \"CSharp\";\r\nConsole.WriteLine(rec[2]);', 'VISESTRUKI_IZBOR', 2, NULL, 1),
(72, 'Rekurzivna metoda je metoda koja poziva samu sebe.', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(73, 'Svaka rekurzivna metoda mora imati uslov za zaustavljanje.', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(74, 'Metoda koja nema neki uslov može uzrokovati grešku.', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(75, 'Koje od sledećih izjava važe za rekurzivne metode?', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(76, 'Šta će ispisati sledeći kod?\r\n\r\nint Faktorijal(int n)\r\n{\r\n    if (n == 0)\r\n        return 1;\r\n    return n * Faktorijal(n - 1);\r\n}\r\n\r\nConsole.WriteLine(Faktorijal(5));', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(77, 'Šta će ispisati sledeći kod?\r\n\r\nint Zbir(int n)\r\n{\r\n    if (n == 0)\r\n        return 0;\r\n    return n + Zbir(n - 1);\r\n}\r\n\r\nConsole.WriteLine(Zbir(4));', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(78, 'Šta će ispisati sledeći kod?\r\n\r\nvoid IspisiTekst(string s, int index)\r\n{\r\n    if (index >= s.Length)\r\n        return;\r\n    Console.Write(s[index]);\r\n    IspisiTekst(s, index + 1);\r\n}\r\n\r\nIspisiTekst(\"ABC\", 0);', 'VISESTRUKI_IZBOR', 3, NULL, 1),
(79, 'Metoda File.WriteAllText() kreira novi fajl ako ne postoji.', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(80, 'File.ReadAllText() čita sadržaj fajla i vraća ga kao string.', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(81, 'File.AppendAllText() briše stari sadržaj fajla pre pisanja.', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(82, 'Šta će ispisati sledeći kod?\r\n\r\nusing System.IO;\r\n\r\nFile.WriteAllText(\"test.txt\", \"Hello World\");\r\nstring sadrzaj = File.ReadAllText(\"test.txt\");\r\nConsole.WriteLine(sadrzaj);', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(83, 'Šta će ispisati sledeći kod?\r\n\r\nusing System.IO;\r\n\r\nif(File.Exists(\"nepostojeci.txt\"))\r\n    Console.WriteLine(\"Fajl postoji\");\r\nelse\r\n    Console.WriteLine(\"Fajl ne postoji\");', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(84, 'Šta predstavlja <T> u generičkim tipovima?', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(85, 'Generička klasa može raditi sa bilo kojim tipom podataka.', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(86, 'Koja je osnovna prednost generika?', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(87, 'Šta će ispisati sledeći kod?\r\n\r\nKutija<int> kutijaBroj = new Kutija<int>();\r\nkutijaBroj.Sadrzaj = 10;\r\n\r\nKutija<string> kutijaRec = new Kutija<string>();\r\nkutijaRec.Sadrzaj = \"Tekst\";\r\n\r\nConsole.WriteLine(kutijaBroj.Sadrzaj + \" \" + kutijaRec.Sadrzaj);', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(88, 'Šta predstavlja LINQ?', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(89, 'Koje ključne reči se koriste u LINQ-upitu?', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(90, 'LINQ se može koristiti samo nad bazama podataka.', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(91, 'U LINQ-upit, ključna reč where se koristi za filtriranje elemenata.', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(92, 'Šta će ispisati sledeći kod?\r\n\r\nList<int> brojevi = new List<int> { 1, 2, 3, 4, 5, 6 };\r\nvar rezultat = brojevi.Where(n => n > 3);\r\n\r\nforeach(var x in rezultat)\r\n    Console.Write(x + \" \");', '', 4, NULL, 1),
(93, 'Koja metoda sortira rezultate u LINQ-u?', 'VISESTRUKI_IZBOR', 4, NULL, 1),
(142, 'awfawfawfaw3232', 'KOD', 5, 'wfafwafwafwafaw', 15);

-- --------------------------------------------------------

--
-- Table structure for table `poglavlje`
--

CREATE TABLE `poglavlje` (
  `poglavlje_id` int(11) NOT NULL,
  `poglavlje_naziv` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `poglavlje`
--

INSERT INTO `poglavlje` (`poglavlje_id`, `poglavlje_naziv`) VALUES
(1, 'Osnove C# jezika i razvojnog okruženja'),
(2, 'Kontrola toka izvršavanja'),
(4, 'Nizovi, kolekcije i rad sa tekstom'),
(5, 'Metode'),
(7, 'Objektno-orijentisano programiranje'),
(15, 'Obrada izuzetaka i grešaka');

-- --------------------------------------------------------

--
-- Table structure for table `potpoglavlje`
--

CREATE TABLE `potpoglavlje` (
  `potpoglavlje_id` int(11) NOT NULL,
  `naziv_potpoglavlja` varchar(100) NOT NULL,
  `poglavlje_id` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `potpoglavlje`
--

INSERT INTO `potpoglavlje` (`potpoglavlje_id`, `naziv_potpoglavlja`, `poglavlje_id`) VALUES
(5, 'Uvod u C# i .NET okruženje', 1),
(6, 'Sintaksa, promenljive i tipovi podataka', 1),
(7, 'Uslovna grananja', 2),
(8, 'Petlje i ponavljanja', 2),
(9, 'Rad sa stringovima i klasom', 4),
(11, 'Jednodimenzionalni i višedimenzionalni nizovi', 4),
(13, 'Kontrola toka izvršavanja', 2),
(14, 'Uslovna grananja', 2),
(18, 'Rukovanje izuzecima i try-catch blokovi', 15),
(19, 'Uvod u objekte', 1),
(20, 'Kolekcije i strukture podataka', 4);

-- --------------------------------------------------------

--
-- Table structure for table `rezultati`
--

CREATE TABLE `rezultati` (
  `rezultat_id` int(11) NOT NULL,
  `korisnik_id` int(11) NOT NULL,
  `nivo_id` int(11) DEFAULT NULL,
  `tip_aktivnosti` enum('KVIZ','TEST') NOT NULL,
  `osvojeni_poeni` int(11) DEFAULT 0,
  `ukupno_poena` int(11) DEFAULT 0,
  `datum_polaganja` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `rezultati`
--

INSERT INTO `rezultati` (`rezultat_id`, `korisnik_id`, `nivo_id`, `tip_aktivnosti`, `osvojeni_poeni`, `ukupno_poena`, `datum_polaganja`) VALUES
(2, 1, 1, 'TEST', 4, 10, '2026-03-19 19:49:01'),
(3, 1, 1, 'TEST', 0, 9, '2026-03-19 19:51:26'),
(13, 1, 1, 'TEST', 0, 10, '2026-05-01 17:55:30'),
(14, 1, 1, 'TEST', 0, 9, '2026-05-02 19:47:40'),
(15, 1, 1, 'TEST', 0, 9, '2026-05-02 19:48:38'),
(16, 1, 1, 'TEST', 0, 10, '2026-05-02 19:49:10'),
(17, 1, 1, 'TEST', 8, 10, '2026-05-02 19:49:27'),
(18, 1, 1, 'TEST', 8, 10, '2026-05-02 19:49:33'),
(19, 95, 1, 'TEST', 7, 10, '2026-05-02 19:49:36'),
(20, 1, 2, 'TEST', 0, 9, '2026-05-12 14:28:22'),
(21, 1, 2, 'TEST', 7, 11, '2026-05-12 16:29:33'),
(23, 1, 2, 'TEST', 7, 11, '2026-05-12 16:33:24'),
(24, 1, 2, 'TEST', 5, 11, '2026-05-12 16:42:05'),
(25, 1, 2, 'TEST', 10, 11, '2026-05-12 16:42:50'),
(26, 1, 2, 'TEST', 5, 11, '2026-05-12 16:49:35'),
(27, 1, 2, 'TEST', 5, 11, '2026-05-12 17:30:14'),
(28, 1, 2, 'TEST', 5, 11, '2026-05-12 17:30:52'),
(29, 1, 2, 'TEST', 2, 11, '2026-05-12 17:31:05'),
(30, 1, 2, 'TEST', 7, 11, '2026-05-12 17:31:19'),
(31, 1, 2, 'TEST', 0, 11, '2026-05-12 17:57:52'),
(34, 1, 3, 'TEST', 5, 12, '2026-05-13 16:20:46'),
(35, 1, 3, 'TEST', 12, 12, '2026-05-13 16:30:11'),
(36, 1, 3, 'TEST', 10, 60, '2026-05-13 16:33:12'),
(37, 1, 21, 'TEST', 0, 0, '2026-06-18 21:59:44'),
(38, 1, 21, 'TEST', 0, 0, '2026-06-18 21:59:47'),
(39, 1, 21, 'TEST', 0, 5, '2026-06-18 22:00:40'),
(41, 48, 1, 'TEST', 2, 50, '2026-07-01 01:41:25'),
(46, 1, NULL, 'KVIZ', 1, 0, '2026-07-01 15:06:15'),
(47, 1, NULL, 'KVIZ', 3, 0, '2026-07-01 15:06:30'),
(48, 1, NULL, 'KVIZ', 0, 5, '2026-07-01 16:08:43'),
(49, 1, NULL, 'KVIZ', 4, 9, '2026-07-01 16:09:15'),
(50, 1, NULL, 'KVIZ', 2, 5, '2026-07-01 16:31:20'),
(51, 1, NULL, 'KVIZ', 2, 5, '2026-07-01 17:35:07'),
(52, 1, NULL, 'KVIZ', 2, 7, '2026-07-02 19:11:44'),
(53, 48, 1, 'TEST', 7, 50, '2026-07-02 20:30:27'),
(54, 27, 5, 'KVIZ', 1, 5, '2026-07-06 00:00:00'),
(55, 48, 1, 'TEST', 5, 50, '2026-07-08 17:32:51'),
(56, 48, NULL, 'KVIZ', 3, 9, '2026-07-08 17:33:32'),
(57, 48, 1, 'TEST', 7, 50, '2026-07-09 14:14:02'),
(58, 1, 21, 'TEST', 7, 50, '2026-07-09 14:43:34'),
(59, 1, 21, 'TEST', 45, 50, '2026-07-09 14:45:39'),
(60, 1, 21, 'TEST', 40, 50, '2026-07-09 14:46:13'),
(61, 1, 4, 'TEST', 45, 50, '2026-07-09 14:46:35'),
(62, 1, 4, 'TEST', 45, 50, '2026-07-09 14:57:34'),
(63, 1, 21, 'TEST', 0, 50, '2026-07-09 15:00:27'),
(64, 1, NULL, 'KVIZ', 3, 7, '2026-07-10 20:48:50'),
(65, 1, NULL, 'KVIZ', 2, 7, '2026-07-10 20:48:55'),
(66, 1, NULL, 'KVIZ', 2, 7, '2026-07-10 21:05:00'),
(67, 100, 1, 'TEST', 7, 50, '2026-07-30 19:27:54'),
(69, 100, 1, 'TEST', 5, 50, '2026-08-06 00:52:47'),
(74, 100, 1, 'TEST', 2, 50, '2026-08-07 15:47:17'),
(75, 100, 1, 'TEST', 25, 50, '2026-08-07 15:47:46'),
(76, 100, 1, 'TEST', 10, 50, '2026-08-07 15:50:02'),
(77, 100, 1, 'TEST', 10, 50, '2026-08-07 15:50:53'),
(78, 100, 1, 'TEST', 0, 50, '2026-08-07 15:52:51'),
(79, 100, 1, 'TEST', 0, 50, '2026-08-07 15:53:56'),
(80, 100, 1, 'TEST', 0, 50, '2026-08-07 15:54:15'),
(81, 103, NULL, 'KVIZ', 0, 9, '2026-08-20 23:36:02'),
(82, 103, NULL, 'KVIZ', 3, 9, '2026-08-20 23:36:17'),
(84, 47, 2, 'TEST', 30, 55, '2026-10-30 22:32:10'),
(85, 1, 3, 'KVIZ', 1, 1, '2026-10-09 00:00:00'),
(87, 1, NULL, 'KVIZ', 1, 6, '2026-11-02 00:15:10'),
(89, 1, NULL, 'KVIZ', 0, 6, '2026-11-02 17:43:55'),
(90, 1, NULL, 'KVIZ', 3, 6, '2026-11-02 17:44:00'),
(91, 48, 1, 'TEST', 25, 50, '2026-11-02 17:45:56'),
(92, 48, 1, 'TEST', 5, 50, '2026-11-02 17:47:37'),
(93, 48, 1, 'TEST', 5, 50, '2026-11-02 17:49:06'),
(94, 48, 1, 'TEST', 5, 50, '2026-11-02 17:53:41'),
(95, 48, 1, 'TEST', 5, 50, '2026-11-02 17:55:16'),
(96, 48, 1, 'TEST', 5, 50, '2026-11-02 17:55:28'),
(97, 125, 1, 'TEST', 7, 50, '2026-11-02 17:55:53');

-- --------------------------------------------------------

--
-- Table structure for table `uloge`
--

CREATE TABLE `uloge` (
  `uloga_id` int(11) NOT NULL,
  `uloga_naziv` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `uloge`
--

INSERT INTO `uloge` (`uloga_id`, `uloga_naziv`) VALUES
(1, 'Admin'),
(2, 'Profesor'),
(3, 'Student');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `korisnici`
--
ALTER TABLE `korisnici`
  ADD PRIMARY KEY (`korisnik_id`),
  ADD KEY `FK_Korisnik_Uloga` (`uloga_id`),
  ADD KEY `fk_korisnik_nivo` (`nivoId`),
  ADD KEY `FK_Korisnici_Profesor` (`profesor_id`);

--
-- Indexes for table `kvizovi`
--
ALTER TABLE `kvizovi`
  ADD PRIMARY KEY (`kviz_id`),
  ADD KEY `korisnik_id` (`korisnik_id`);

--
-- Indexes for table `kviz_pitanja`
--
ALTER TABLE `kviz_pitanja`
  ADD PRIMARY KEY (`kviz_pitanje_id`),
  ADD KEY `kviz_id` (`kviz_id`),
  ADD KEY `pitanje_id` (`pitanje_id`);

--
-- Indexes for table `lekcija`
--
ALTER TABLE `lekcija`
  ADD PRIMARY KEY (`lekcija_id`),
  ADD KEY `PoglavljeID` (`poglavlje_id`),
  ADD KEY `PotpoglavljeID` (`potpoglavlje_id`),
  ADD KEY `NivoID` (`nivo_id`),
  ADD KEY `FK_Lekcija_Korisnik` (`korisnik_id`);

--
-- Indexes for table `nivoi`
--
ALTER TABLE `nivoi`
  ADD PRIMARY KEY (`NivoId`);

--
-- Indexes for table `odgovori`
--
ALTER TABLE `odgovori`
  ADD PRIMARY KEY (`odgovor_id`),
  ADD KEY `PitanjeID` (`pitanje_id`);

--
-- Indexes for table `pitanja`
--
ALTER TABLE `pitanja`
  ADD PRIMARY KEY (`pitanje_id`),
  ADD KEY `NivoID` (`nivo_id`),
  ADD KEY `FK_Pitanja_Korisnik` (`korisnik_id`);

--
-- Indexes for table `poglavlje`
--
ALTER TABLE `poglavlje`
  ADD PRIMARY KEY (`poglavlje_id`);

--
-- Indexes for table `potpoglavlje`
--
ALTER TABLE `potpoglavlje`
  ADD PRIMARY KEY (`potpoglavlje_id`),
  ADD KEY `PoglavljeID` (`poglavlje_id`);

--
-- Indexes for table `rezultati`
--
ALTER TABLE `rezultati`
  ADD PRIMARY KEY (`rezultat_id`),
  ADD KEY `korisnik_id` (`korisnik_id`),
  ADD KEY `nivo_id` (`nivo_id`);

--
-- Indexes for table `uloge`
--
ALTER TABLE `uloge`
  ADD PRIMARY KEY (`uloga_id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `korisnici`
--
ALTER TABLE `korisnici`
  MODIFY `korisnik_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=127;

--
-- AUTO_INCREMENT for table `kvizovi`
--
ALTER TABLE `kvizovi`
  MODIFY `kviz_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=30;

--
-- AUTO_INCREMENT for table `kviz_pitanja`
--
ALTER TABLE `kviz_pitanja`
  MODIFY `kviz_pitanje_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=193;

--
-- AUTO_INCREMENT for table `lekcija`
--
ALTER TABLE `lekcija`
  MODIFY `lekcija_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=70;

--
-- AUTO_INCREMENT for table `nivoi`
--
ALTER TABLE `nivoi`
  MODIFY `NivoId` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=28;

--
-- AUTO_INCREMENT for table `odgovori`
--
ALTER TABLE `odgovori`
  MODIFY `odgovor_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=391;

--
-- AUTO_INCREMENT for table `pitanja`
--
ALTER TABLE `pitanja`
  MODIFY `pitanje_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=144;

--
-- AUTO_INCREMENT for table `poglavlje`
--
ALTER TABLE `poglavlje`
  MODIFY `poglavlje_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;

--
-- AUTO_INCREMENT for table `potpoglavlje`
--
ALTER TABLE `potpoglavlje`
  MODIFY `potpoglavlje_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=23;

--
-- AUTO_INCREMENT for table `rezultati`
--
ALTER TABLE `rezultati`
  MODIFY `rezultat_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=98;

--
-- AUTO_INCREMENT for table `uloge`
--
ALTER TABLE `uloge`
  MODIFY `uloga_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=23;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `korisnici`
--
ALTER TABLE `korisnici`
  ADD CONSTRAINT `FK_Korisnici_Profesor` FOREIGN KEY (`profesor_id`) REFERENCES `korisnici` (`korisnik_id`) ON DELETE SET NULL,
  ADD CONSTRAINT `FK_Korisnik_Uloga` FOREIGN KEY (`uloga_id`) REFERENCES `uloge` (`uloga_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_korisnik_nivo` FOREIGN KEY (`nivoId`) REFERENCES `nivoi` (`NivoId`) ON UPDATE CASCADE;

--
-- Constraints for table `kvizovi`
--
ALTER TABLE `kvizovi`
  ADD CONSTRAINT `kvizovi_ibfk_1` FOREIGN KEY (`korisnik_id`) REFERENCES `korisnici` (`korisnik_id`);

--
-- Constraints for table `kviz_pitanja`
--
ALTER TABLE `kviz_pitanja`
  ADD CONSTRAINT `kviz_pitanja_ibfk_1` FOREIGN KEY (`kviz_id`) REFERENCES `kvizovi` (`kviz_id`) ON DELETE CASCADE,
  ADD CONSTRAINT `kviz_pitanja_ibfk_2` FOREIGN KEY (`pitanje_id`) REFERENCES `pitanja` (`pitanje_id`) ON DELETE CASCADE;

--
-- Constraints for table `lekcija`
--
ALTER TABLE `lekcija`
  ADD CONSTRAINT `FK_Lekcija_Korisnik` FOREIGN KEY (`korisnik_id`) REFERENCES `korisnici` (`korisnik_id`) ON DELETE CASCADE,
  ADD CONSTRAINT `lekcija_ibfk_1` FOREIGN KEY (`poglavlje_id`) REFERENCES `poglavlje` (`poglavlje_id`) ON DELETE SET NULL,
  ADD CONSTRAINT `lekcija_ibfk_2` FOREIGN KEY (`potpoglavlje_id`) REFERENCES `potpoglavlje` (`potpoglavlje_id`) ON DELETE SET NULL,
  ADD CONSTRAINT `lekcija_ibfk_3` FOREIGN KEY (`nivo_id`) REFERENCES `nivoi` (`NivoId`);

--
-- Constraints for table `odgovori`
--
ALTER TABLE `odgovori`
  ADD CONSTRAINT `odgovori_ibfk_1` FOREIGN KEY (`pitanje_id`) REFERENCES `pitanja` (`pitanje_id`);

--
-- Constraints for table `pitanja`
--
ALTER TABLE `pitanja`
  ADD CONSTRAINT `FK_Pitanja_Korisnik` FOREIGN KEY (`korisnik_id`) REFERENCES `korisnici` (`korisnik_id`) ON DELETE CASCADE,
  ADD CONSTRAINT `pitanja_ibfk_1` FOREIGN KEY (`nivo_id`) REFERENCES `nivoi` (`NivoId`);

--
-- Constraints for table `potpoglavlje`
--
ALTER TABLE `potpoglavlje`
  ADD CONSTRAINT `potpoglavlje_ibfk_1` FOREIGN KEY (`poglavlje_id`) REFERENCES `poglavlje` (`poglavlje_id`) ON DELETE SET NULL;

--
-- Constraints for table `rezultati`
--
ALTER TABLE `rezultati`
  ADD CONSTRAINT `rezultati_ibfk_1` FOREIGN KEY (`korisnik_id`) REFERENCES `korisnici` (`korisnik_id`) ON DELETE CASCADE,
  ADD CONSTRAINT `rezultati_ibfk_2` FOREIGN KEY (`nivo_id`) REFERENCES `nivoi` (`NivoId`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
