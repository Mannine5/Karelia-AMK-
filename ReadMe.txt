# Invoicing Application

Connection String -asetukset on m��ritelty InvoiceRepository.cs tiedostossa kahteen erilliseen vakioon.

## Yleiskuvaus
T�m� on laskutussovellus, joka mahdollistaa laskujen luomisen, muokkaamisen ja hallinnan. 
Sovelluksessa voi my�s hallita asiakkaita, tuotteita ja ty�rivej�. Se tukee dynaamista kokonaishinnan laskentaa ilman tietokantaan tallennettua hintayhteenvetoa.

## Ominaisuudet
- Laskujen luominen, p�ivitt�minen ja poisto
- Asiakkaiden lis�ys, muokkaus ja poisto
- Tuotteiden ja ty�rivin hallinta
- Dynaaminen kokonaishinnan laskenta
- Reaaliaikainen p�ivitys tapahtumak�sittelij�iden avulla
- Tietokannan automaattinen luonti ja esimerkkidatan lis�ys

## K�ytt�ohjeet
- Laskun luonti: Klikkaa "Luo uusi lasku" ja sy�t� tarvittavat tiedot.
- Tuote- ja ty�riveill�, jos poistat ohjelman juuri luoman uuden rivin, saat uuden aktivoimalla jonkin olemassa olevan rivin ja painamalla kaksi kertaa Enter ja klikkaamalla hiirell� rivi�.
- Laskun p�ivitt�minen: Valitse olemassa oleva lasku ja klikkaa "Update".
- Laskuille, tuotteille ja asiakkaiden profiiliin p��see kaksoiklikkaamalla suoraan Show All -ikkunoista.
- Asiakkaiden hallinta: Voit lis�t�, muokata ja poistaa asiakkaita.
- Tuotteiden : Voit lis�t�, muokata ja poistaa tuotteita.

## Tunnetut ongelmat
- Dynaaminen hinnan laskenta voi viiv�sty� suurella datam��r�ll�.
- SQL-kyselyiden optimointi voi parantaa suorituskyky�.

## Mahdolliset kehitt�misen kohteet
- Lis�� tapahtumapohjaisia p�ivityksi�
- Tietokantaan tallennus olisi hyv� toteuttaa asynkronisesti.



