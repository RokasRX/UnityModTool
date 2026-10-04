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
| Eksportavimas | Galimybė žaidimų tūrinį eksportuoti patogiais formatais naudojimui kitose vietose | Pagalbinė funkcija |
| Importavimas | Galimybė importuoti tūrinį iš patogaus formato | Pagalbinė funkcija |
| Redagavimas | Galimybė redaguoti žaidimo tūrinį norint pakeisti išvaizdą ar veikimą | Pagrindinis modulis |

**Į kursinio darbo apimtį neįeina:** Failų nuskaitymo, rašymo, redagavimo logika, tam jau yra sukurta biblioteka

## 3. Pagrindinis modulis

**Pavadinimas ir atsakomybė:** Ilgas Unity žaidimų variklio modifikacijų kūrimo procesas

**Logika, kurią reikės projektuoti ir testuoti:** Reikia tikrinti ar importuojamo tūrinio variklio versijos sutampta su dabartiniu projektu, ar kintamujų tipai sutampa redaguojant. Tikrinama kaip išdėstyti kai kurių komponentų duomenys, kad jų redagavimas būtų atliktas teisingai

**Įvestis:** Unity žaidimo tūrinio failai, pavyzdžiui sharedassets0.assets kuris laiko Unity žaidimų pirmos scenos vizualų tūrinį

**Išvestis:** Unity žaidimo tūrinio failai, tai gali būti tas pats sharedassets0.assets failas su atliktomis modifikacijomis

**Veikimo eiga:** Pasirenkama kūrį failą norima įkelti ir redaguoti, atliekami pakeitimai norimai modifikacijai sukurti, failas su pakeitimais yra išsaugomas ir gali būti naudojamas pakeisti žaidimo tūriniui

### Taisyklės arba sprendimo žingsniai

1. Žaidimo tūrinys gali būti eksportuojamas jam įprastais formatais (pvz 3d modelis kaip .obj failas)
2. Žaidimo tūrinys importuojamas iš jam įprastų formatų (pvz tekstūra kaip .png failas)
3. Žaidimo tūrinys lengvai ir greitai redaguojamas keičiant reikalingus jo laukus

### Scenarijai būsimiems testams

| Scenarijus | Pradinės sąlygos ir konkreti įvestis | Veiksmas | Tikslus laukiamas rezultatas |
|---|---|---|---|
| Įprastas atvejis | Pasirenkamas žaidėjo greičio laukas ir įrašoma reikšmė 100 | Patvirtinamas keitimas | Reikšmė pakeičiama ir yra paruošta išsaugojimui |
| Ribinis atvejis arba konfliktas | Pasirenkamas žaidėjo greičio laukas ir įrašoma reikšmė "ABC" | Patvirtinamas keitimas | Parodoma ispėjimas dėl bandymo įvesti String tipo reikšmę lauke kuris laiko skaičių ir prašoma įvesti kitą reikšmę |
| Klaida arba neįmanomas rezultatas | Pasirenkamas žaidėjo greičio laukas ir įrašoma reikšmė 5000000000 | Patvirtinamas keitimas | Parodoma klaida, nes įvesta reikšmė yra didesnė nei keičiamo lauko didžiausia galima reikšmė |

**Jei modulis naudoja AI:** Netaikoma

## 4. Kokybės atributas

**Pasirinktas atributas:** Darbo greitis

**Kodėl svarbus šiai sistemai:** Didžiausia problema kuriant modifikacijas yra būtinybė "šokinėti" tarp daug skirtingų programų

**Tikrinimo scenarijus ir sąlygos:** Kuriama pakankamai paprasta žaidimo modifikacija kuri pakeičia kelis skirtingus tūrinio tipus, tokius kaip 3d modeliai ir tekstūros

**Sėkmės kriterijus:** Laikas sukurti modifikacijai matomai sumažėja tiek, kad to negalima skaityti kaip natūralaus skirtumo

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
