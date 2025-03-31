# Invoicing Application

Connection String -asetukset on määritelty InvoiceRepository.cs tiedostossa kahteen erilliseen vakioon.

## Yleiskuvaus
Tämä on laskutussovellus, joka mahdollistaa laskujen luomisen, muokkaamisen ja hallinnan. 
Sovelluksessa voi myös hallita asiakkaita, tuotteita ja työrivejä. Se tukee dynaamista kokonaishinnan laskentaa ilman tietokantaan tallennettua hintayhteenvetoa.

## Ominaisuudet
- Laskujen luominen, päivittäminen ja poisto
- Asiakkaiden lisäys, muokkaus ja poisto
- Tuotteiden ja työrivin hallinta
- Dynaaminen kokonaishinnan laskenta
- Reaaliaikainen päivitys tapahtumakäsittelijöiden avulla
- Tietokannan automaattinen luonti ja esimerkkidatan lisäys

## Käyttöohjeet
- Laskun luonti: Klikkaa "Luo uusi lasku" ja syötä tarvittavat tiedot.
- Tuote- ja työriveillä, jos poistat ohjelman juuri luoman uuden rivin, saat uuden aktivoimalla jonkin olemassa olevan rivin ja painamalla kaksi kertaa Enter ja klikkaamalla hiirellä riviä.
- Laskun päivittäminen: Valitse olemassa oleva lasku ja klikkaa "Update".
- Laskuille, tuotteille ja asiakkaiden profiiliin pääsee kaksoiklikkaamalla suoraan Show All -ikkunoista.
- Asiakkaiden hallinta: Voit lisätä, muokata ja poistaa asiakkaita.
- Tuotteiden : Voit lisätä, muokata ja poistaa tuotteita.

## Tunnetut ongelmat
- Dynaaminen hinnan laskenta voi viivästyä suurella datamäärällä.
- SQL-kyselyiden optimointi voi parantaa suorituskykyä.

## Mahdolliset kehittämisen kohteet
- Lisää tapahtumapohjaisia päivityksiä
- Tietokantaan tallennus olisi hyvä toteuttaa asynkronisesti.



