# Unity Mod Tool

Kursinio darbo I dalis: projektavimo dokumentas

## 1. Problema ir idėja

**Sistema vienu sakiniu:** Sistema skirta kurti Unity žaidimų modifikacijas ir palengvinti, pagreitinti jų kūrimo procesą

**Problema ir dabartinis procesas:** Dabar naudojami UABE arba UABEA įrankiai, tačiau tam reikia taip pat naudoti Unity žaidimų variklį, kad sukurti kai kuriuos failus.

**Nauda:** Modifikacijų kūrimo greitis, patogumas

**Naudotojai:** Sistema naudosis patyrę modifikacijų kūrėjai kurdami modifikacijas, žmonės norėdami išmokti kurti modifikacijas

**Prielaidos:** Žinau daug apie Unity žaidimų modifikacijas, tačiau tikslus failų duomenų išdėtymas/šifravimas yra prielaida

## 2. Apimtis

| Funkcija | Ką naudotojas galės atlikti | Pagrindinis modulis ar pagalbinė funkcija |
|---|---|---|
| Peržiūra | Galimybė greitai peržiūrėti žaidimų tūrinį programos viduje | Pagrindinis modulis |
| Eksportavimas | Galimybė žaidimų tūrinį eksportuoti patogiais formatais naudojimui kitose vietose | Pagrindinis modulis |
| Importavimas | Galimybė importuoti tūrinį iš patogaus formato | Pagrindinis modulis |
| Redagavimas | Galimybė redaguoti žaidimo tūrinį norint pakeisti išvaizdą ar veikimą | Pagrindinis modulis |

**Į kursinio darbo apimtį neįeina:** Failų nuskaitymo, rašymo, redagavimo logika, tam jau yra sukurta biblioteka

## 3. Pagrindinis modulis

**Pavadinimas ir atsakomybė:** Importavimas/Eksportavimas, atsakingas už tūrinio importavimą/eksportavimą iš/į patogius formatus

**Logika, kurią reikės projektuoti ir testuoti:** 3d modelių, tekstūrų, garso, teksto tūrinio importavimas/ekportavimas

**Įvestis:** Vartotojo kurto tūrinio failas kuris yra importuojamas arba pasirinktas žaidimo tūrinis kuris yra eksportuojamas

**Išvestis:** Žaidimo failas su importuotu vartotojo tūriniu arba tūrinio failas kuris eksportuotas iš žaidimo

**Veikimo eiga:** Norint importuoti vartotojo tūrinio failą pasirenkama kūrį žaidimo tūrinį norima pakeisti, patvirtinus importavimo veiksmą programa vartotojo tūrinio duomenis paverčia į reikiamą formatą ir pakeičia žaidimo tūrinį su naujais duomenimis. Norint eksportuoti žaidimo tūrinį pasirenkama kuris tūrinys turi būti eksportuojamas, pasirenkama į kur jį eksportuoti, tada programa duomenis paverčia į tokio tipo tūriniui būdingą formatą ir jį rašo į pasirinktą vietą

### Taisyklės arba sprendimo žingsniai

1. Žaidimo tūrinys gali būti eksportuojamas jam įprastais formatais (pvz 3d modelis kaip .obj failas)
2. Žaidimo tūrinys importuojamas iš jam įprastų formatų (pvz tekstūra kaip .png failas)
3. Žaidimo tūrinys gali būti importuojamas/eksportuojamas kaip binary failas suteikiant galimybę dirbti sena darbo eiga.
4. Žaidimo tūrinys gali būti importuojamas/eksportuojamas kaip json failas suteikiant galimybę dirbti sena darbo eiga.

### Scenarijai būsimiems testams

| Scenarijus | Pradinės sąlygos ir konkreti įvestis | Veiksmas | Tikslus laukiamas rezultatas |
|---|---|---|---|
| Įprastas atvejis | Pasirinktas žaidimo 3d modelis ir bandomas pakeisti kitu 3d modeliu | Patvirtinamas keitimas | Modelis pakeičiamas ir žaidimo failas yra paruoštas išsaugojimui |
| Ribinis atvejis arba konfliktas | Pasirinktas žaidimo 3d modelis ir jo vietoje bandoma importuoti neteisingai redaguotas json failas | Patvirtinamas keitimas | Jei json failą įmanoma teisingai nuskaityti duomenys vistiek yra įrašomi, tačiau naudojant šį tūrinį žaidime gali kilti problemų, pavyzdžiui jei json faile nurodyta, kad modelis turi naudoti 16-bitų indeksus, bet indeksų yra daugiau nei 2^16 |
| Klaida arba neįmanomas rezultatas | Pasirinktas žaidimo 3d modelis ir jo vietoje bandoma importuoti kito tipo failas | Patvirtinamas keitimas | Parodoma klaida, nes pasirinktas failas negali būti nuskaitytas ir įrašytas kaip 3d modelis |

**Jei modulis naudoja AI:** Netaikoma

## 4. Kokybės atributas

**Pasirinktas atributas:** Darbo greitis

**Kodėl svarbus šiai sistemai:** Didžiausia problema kuriant modifikacijas yra būtinybė "šokinėti" tarp daug skirtingų programų

**Tikrinimo scenarijus ir sąlygos:** Kuriama pakankamai paprasta žaidimo modifikacija kuri pakeičia kelis skirtingus tūrinio tipus, tokius kaip 3d modeliai ir tekstūros

**Sėkmės kriterijus:** Laikas sukurti modifikacijai sumažėja bent dvigubai

**Numatytas projektavimo sprendimas:** Būdai importuoti žaidimo tūrinį iš patogių formatų

**Kaip patikrinsiu vėlesniame etape:** Kuriant tokią pat modifikaciją su mano kurta programa ir standartiniais įrankiais ir lyginant kiek laiko užtrunka kiekvieną iš jų sukurti

**Sprendimo kaina arba ribojimas:** Reikia pridėti daug papildomos logikos failų importavimui/eksportavimui

## 5. Pradinė sistemos struktūra

### Paprasta schema

* Vaizdinis modulis
    * Pagrindinis modulis
        * Importavimo modulis
        * Eksportavimo modulis
        * Redagavimo modulis

| Sistemos dalis | Atsakomybė |
|---|---|
| Vaizdinis modulis | Rodo visus veiksmus kuriuos gali atlikti vartotojas, gali rodyti pasirinkto žaidimo tūrinio "preview" |
| Pagrindinis modulis | Leidžia įkelti žaidimo failus kuriuos norima modifikuoti, išsaugoti juos po jų pakeitimo |
| Importavimo modulis | Paverčia naudotojo įkeltą tūrinį į žaidimų varikliui reikalingą formatą |
| Eksportavimo modulis | Paverčia žaidimo tūrinį į jam būdingus failų tipus ir juos išsaugo |
| Redagavimo modulis | Nuskaito duomenis iš žaidimo failų ir duoda juos laisvai redaguoti |

**Planuojamos technologijos ir pasirinkimo priežastys:** Programavimo kalba: C#, pasirenkama dėl to, kad turi geriausią Unity žaidimų variklio failų modifikavimo biblioteką. Vartotojo sąsajai bus naudojama Avalonia biblioteka kuri yra vieną iš standartinų bibliotekų kuriant UI, ji veikia ant visų platformų ir jos išdėstymas gali būti keičiamas ne tik statiniuose .axaml failuose, bet ir pačiame С# kode.

## 6. AI panaudojimas

### AI rengiant šį dokumentą

Nenaudojau

### Planuojamas AI naudojimas kuriant sistemą

**Kur ir kam naudosiu AI:** AI būtų naudojamas greitam projekto rėmo padarymui, kuris po to būtų plečiamas ranka rašant kodą. Taip pat greitesniam įrankių supratimui, dokumentacijos naršymui.

**Kaip tikrinsiu pasiūlymus ir sugeneruotą kodą:** Lyginant su naudojamų bibliotekų dokumentacijoje pateiktais pavyzdžiais, C# bei objektinio programavimo standartais

**Ar AI bus sistemos funkcionalumo dalis:** Ne

## 7. Tolesnių darbų planas

| Darbas | Apčiuopiamas rezultatas | Planuojama darbų seka |
|---|---|---|
| UI išdėstymo kūrimas | Patogus naudojimui UI išdėstymas | 1 |
| Failų nuskaitymas | Duomenys iš įkeltų failų rodomi programos UI | 2 |
| Reikšmių redagavimas | Reikšmes galima pakeisti kitomis | 3 |
| Failų rašymas | Pakeisti failai yra išsaugoti su visais pakeitimais | 4 |
| Tūrinio "preview" | Pasirinktą tūrinį galima matyti "preview" lange | 5 |
| Tūrinio eksportavimas | Pasirinktą tūrinį galima eksportuoti jam būdingu formatu | 6 |
| Tūrinio importavimas | Pasirinktą tūrinį galima importuoti iš jam būdingo formato | 7 |

**Būsimo prototipo veikimo scenarijus:** Pakeisti 3d modelį žaidime, į programą ikeliant žaidimo failą, pasirenkant modelį kurį norima pakeisti ir importuojant naują modelį. Tikimasi pamatyti naujai importuotą modelį ikeltą žaidime

| Rizika arba neaiškumas | Kaip patikrinsiu arba sumažinsiu |
|---|---|
| Paslėpta žaidimo variklio versija failuose | Žaidimo versija privalo būti kai kuriuose failuose, kad žaidimas veiktų, todėl galima versijos numerį rasti juose |
| Neįprastas komponentų laukų išdėstymas failuose | Programa bus testuojama ant kiek įmanoma daugiau žaidimų, bus logika bandyti automatiškai prisitaikyti prie tam tikrų formatų, vartotojai gali pasiūlyti kaip sutvarkyti dar nepalaikomų žaidimų/komponentų formatų nuskaitymą |

## Šaltiniai, jei naudojote
