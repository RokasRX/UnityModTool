# Unity Mod Tool

Kursinio darbo I dalis: projektavimo dokumentas

Šiame dokumente „žaidimo ištekliais“ vadinamos žaidime naudojamos dalys, pavyzdžiui, 3D modeliai, tekstūros (paviršiaus vaizdai), garsai ir tekstas. Programa skirta šiuos išteklius peržiūrėti, iškelti iš žaidimo failų, pakeisti ir įkelti atgal.

## 1. Problema ir idėja

**Sistema vienu sakiniu:** Programa padės kurti Unity žaidimų modifikacijas – keisti žaidimo išteklius vienoje vietoje, kad tai būtų patogiau ir užtruktų mažiau laiko.

**Problema ir dabartinis procesas:** Šiuo metu žaidimo failams peržiūrėti ir keisti naudojami UABE arba UABEA įrankiai. Kai kuriuos pakeistus išteklius, prieš įkeliant juos į žaidimą, taip pat reikia paruošti Unity projekte. Dėl to tenka naudoti kelias skirtingas programas ir kartoti papildomus veiksmus.

**Projekto iššūkis:** Vienas svarbiausių iššūkių – išsiaiškinti, kaip kiekvienas žaidimas saugo 3D modelių duomenis ir kaip juos teisingai perskaityti bei pakeisti. Net tame pačiame žaidime skirtingi modeliai, o kartais ir viename faile esantys modeliai, gali būti aprašyti nevienodai. Pavyzdžiui, kai kurie modeliai turi kelis UV kanalus – tai papildomi duomenys, nusakantys, kaip tekstūra (modelio paviršiaus vaizdas) uždedama ant modelio. Todėl programa negali tikėtis, kad visi modeliai sudaryti vienodai: jai reikės atpažinti jų duomenų sandarą ir išsaugoti ją teisingai, kad pakeistas modelis liktų tinkamas žaidimui.

**Nauda:** Modifikacijas bus patogiau kurti, o jų kūrimas turėtų užtrukti trumpiau, nes pagrindinius išteklių peržiūros, keitimo ir perkėlimo veiksmus bus galima atlikti vienoje programoje.

**Naudotojai:** Programa skirta patyrusiems modifikacijų kūrėjams ir žmonėms, kurie nori išmokti kurti modifikacijas.

**Prielaidos:** Turiu daug žinių apie Unity žaidimų modifikacijas. Tačiau tiksli informacija apie tai, kaip skirtingų žaidimų failuose išdėstyti arba apsaugoti duomenys, kol kas yra prielaida ir turės būti tikrinama.

## 2. Apimtis

| Funkcija | Ką naudotojas galės atlikti | Pagrindinis modulis ar pagalbinė funkcija |
|---|---|---|
| Peržiūra | Peržiūrėti žaidimo išteklius programoje, įskaitant 3D modelius, tekstūras, garsus ir tekstą | Pagrindinis modulis |
| Eksportavimas | Iškelti žaidimo išteklių iš žaidimo failo ir išsaugoti jį patogiu formatu, kurį galima naudoti kitose programose | Pagrindinis modulis |
| Importavimas | Įkelti išteklių iš patogaus formato ir panaudoti jį pasirinktam žaidimo ištekliui pakeisti | Pagrindinis modulis |
| Redagavimas | Keisti žaidimo išteklių, kad būtų galima pakeisti žaidimo išvaizdą ar veikimą | Pagrindinis modulis |

**Į kursinio darbo apimtį neįeina:** Žemo lygio logika, kuri nuskaito, įrašo ir keičia žaidimo failų vidinę sandarą, nebus kuriama iš naujo – tam bus naudojama jau sukurta biblioteka. Kursinio darbo dalis yra programa, kurioje naudotojas galės atlikti išteklių peržiūros, importavimo, eksportavimo ir redagavimo veiksmus.

## 3. Pagrindinis modulis

**Pavadinimas ir atsakomybė:** Importavimo ir eksportavimo modulis. Jis paruoš naudotojo failą taip, kad jį būtų galima panaudoti kaip žaidimo išteklių, arba išsaugos žaidimo išteklių patogiu formatu.

**Logika, kurią reikės projektuoti ir testuoti:** 3D modelių, tekstūrų, garsų ir teksto išteklių importavimas bei eksportavimas. Kiekvienos rūšies ištekliui gali reikėti skirtingo failo formato ir skirtingo paruošimo būdo.

**Įvestis:** Importuojant – naudotojo pasirinktas failas, kurį norima panaudoti žaidime, ir žaidimo išteklius, kurį norima pakeisti. Eksportuojant – naudotojo pasirinktas žaidimo išteklius.

**Išvestis:** Importuojant – pakeistas žaidimo failas, kurį naudotojas galės išsaugoti. Eksportuojant – pasirinkto žaidimo ištekliaus failas, išsaugotas naudotojo nurodytoje vietoje.

**Veikimo eiga:** Norėdamas pakeisti išteklių, naudotojas pasirenka žaidimo failą ir jame esantį išteklių, kurį nori pakeisti. Tuomet pasirenka savo paruoštą failą ir patvirtina importavimą. Programa patikrina, ar failą gali perskaityti, prireikus paverčia jo duomenis į žaidimui reikalingą formatą ir pakeičia pasirinktą išteklių. Pakeistą žaidimo failą naudotojas gali išsaugoti.

Norėdamas eksportuoti išteklių, naudotojas pasirenka jį žaidimo faile ir nurodo, kur išsaugoti. Programa paruošia duomenis pasirinkto tipo ištekliui tinkamu formatu ir išsaugo failą nurodytoje vietoje.

### Taisyklės arba sprendimo žingsniai

1. Išteklius bus galima eksportuoti į jam įprastus formatus, pavyzdžiui, 3D modelį – kaip `.obj` failą.
2. Išteklius bus galima importuoti iš jam įprastų formatų, pavyzdžiui, tekstūrą – kaip `.png` failą.
3. Išteklius bus galima importuoti ir eksportuoti kaip dvejetainį (binary) failą – tokį, kuriame duomenys saugomi kompiuteriui skirtu formatu. Tai leis ir toliau naudoti esamą darbo būdą bei suderinamumą su esamais įrankiais ar biblioteka.
4. Išteklius bus galima importuoti ir eksportuoti kaip JSON failą – tekstinį formatą, kuriame duomenys pateikiami taip, kad juos būtų patogu peržiūrėti ir redaguoti. Tai taip pat leis išlaikyti suderinamumą su esamais įrankiais ar biblioteka.

### Scenarijai būsimiems testams

| Scenarijus | Pradinės sąlygos ir konkreti įvestis | Veiksmas | Tikslus laukiamas rezultatas |
|---|---|---|---|
| Įprastas atvejis | Pasirinktas žaidimo 3D modelis ir kitas 3D modelio failas, kuriuo norima jį pakeisti | Patvirtinamas importavimas | Pasirinktas modelis pakeičiamas nauju, o pakeistą žaidimo failą galima išsaugoti |
| Ribinis atvejis arba konfliktas | Pasirinktas žaidimo 3D modelis ir JSON failas, kurį programa gali perskaityti, bet kurio reikšmės gali būti netinkamos žaidimui. Pavyzdžiui, failas nurodo naudoti modelio taškų numeravimo būdą, galintį aprašyti iki 65 536 taškų, nors modelyje jų yra daugiau. | Patvirtinamas importavimas | Jei JSON failą galima perskaityti, jo duomenys įrašomi, net jei jų reikšmės nėra tinkamos žaidimui. Programa negarantuoja, kad toks išteklius veiks žaidime; jį panaudojus žaidimas gali veikti netinkamai. |
| Klaida arba neįmanomas rezultatas | Pasirinktas žaidimo 3D modelis, o vietoj jo bandoma importuoti kitos rūšies failą, pavyzdžiui, tekstūrą | Patvirtinamas importavimas | Programa parodo klaidą ir nepakeičia modelio, nes pasirinktas failas nėra tinkamas 3D modeliui. |

**Jei modulis naudoja AI:** AI šiam moduliui nenaudojamas. AI panaudojimas rengiant šį dokumentą aprašytas 6 skyriuje.

## 4. Kokybės atributas

**Pasirinktas atributas:** Darbo greitis – laikas, per kurį naudotojas gali sukurti modifikaciją.

**Kodėl svarbus šiai sistemai:** Kuriant modifikacijas šiuo metu dažnai tenka pereiti iš vienos programos į kitą. Tai apsunkina darbą ir užima papildomo laiko.

**Tikrinimo scenarijus ir sąlygos:** Bus sukurta paprasta modifikacija, pakeičianti kelis skirtingus išteklius, pavyzdžiui, 3D modelį ir tekstūrą.

**Sėkmės kriterijus:** Su šia programa tą pačią modifikaciją sukurti užtruks bent perpus trumpiau nei naudojant įprastus įrankius.

**Numatytas projektavimo sprendimas:** Leisti įkelti ir išsaugoti žaidimo išteklius patogiais, jiems įprastais formatais.

**Kaip patikrinsiu vėlesniame etape:** Tą pačią modifikaciją sukursiu ir savo programa, ir įprastais įrankiais. Palyginsiu, kiek laiko užtruko kiekvienas būdas.

**Sprendimo kaina arba ribojimas:** Patogesniems failų formatams palaikyti reikės sukurti papildomą logiką, kuri išteklius paverstų iš vieno formato į kitą.

## 5. Pradinė sistemos struktūra

### Paprasta schema

* Vaizdinis modulis
    * Pagrindinis modulis
        * Importavimo modulis
        * Eksportavimo modulis
        * Redagavimo modulis

| Sistemos dalis | Atsakomybė |
|---|---|
| Vaizdinis modulis | Parodo naudotojui galimus veiksmus ir leidžia peržiūrėti pasirinktą žaidimo išteklių. |
| Pagrindinis modulis | Leidžia atidaryti žaidimo failus, kuriuos norima keisti, ir išsaugoti juos atlikus pakeitimus. |
| Importavimo modulis | Paruošia naudotojo pasirinktą failą, kad jo duomenis būtų galima įkelti į žaidimo failą. |
| Eksportavimo modulis | Iškelia žaidimo išteklių iš žaidimo failo, paruošia jį pasirinktu formatu ir išsaugo. |
| Redagavimo modulis | Nuskaito informaciją apie žaidimo išteklių ir leidžia naudotojui ją keisti. |

**Planuojamos technologijos ir pasirinkimo priežastys:** Programa bus rašoma C# kalba, nes jai yra sukurta tinkama biblioteka Unity žaidimų failams keisti. Vartotojo sąsajai bus naudojama „Avalonia“ – biblioteka, skirta programų langams ir valdikliams kurti. Ji veikia keliose operacinėse sistemose, o sąsajos išdėstymą galima aprašyti atskiruose `.axaml` failuose arba C# kode.

## 6. AI panaudojimas

### AI rengiant šį dokumentą

AI buvo panaudotas trūkstamoms aiškinamosioms detalėms papildyti ir dokumentui padaryti suprantamesnį žmogui, kuris nėra susipažinęs su Unity ar žaidimų modifikavimu. Aiškinimai buvo parengti pagal dokumento autoriaus pateiktą informaciją ir patikslinimus.

### Planuojamas AI naudojimas kuriant sistemą

**Kur ir kam naudosiu AI:** AI gali padėti greičiau sukurti pradinį projekto pagrindą, kurį vėliau plėsiu ir koreguosiu rašydamas kodą. Jį taip pat naudosiu greičiau susipažinti su įrankiais ir rasti reikiamą informaciją jų dokumentacijoje.

**Kaip tikrinsiu pasiūlymus ir sugeneruotą kodą:** Pasiūlymus ir kodą lyginsiu su naudojamų bibliotekų dokumentacijos pavyzdžiais bei tikrinsiu, ar jie atitinka C# kalbos ir objektinio programavimo principus.

**Ar AI bus sistemos funkcionalumo dalis:** Ne. AI nebus įtrauktas į galutinės programos veikimą.

## 7. Tolesnių darbų planas

| Darbas | Apčiuopiamas rezultatas | Planuojama darbų seka |
|---|---|---|
| Vartotojo sąsajos išdėstymo kūrimas | Patogi naudoti programos sąsaja | 1 |
| Failų nuskaitymas | Įkeltų failų duomenys rodomi programos lange | 2 |
| Reikšmių redagavimas | Naudotojas gali pakeisti pasirinktas reikšmes | 3 |
| Failų įrašymas | Pakeisti failai išsaugomi su atliktais pakeitimais | 4 |
| Išteklių peržiūra | Pasirinktą išteklių galima peržiūrėti programos peržiūros lange | 5 |
| Išteklių eksportavimas | Pasirinktą išteklių galima išsaugoti jam įprastu formatu | 6 |
| Išteklių importavimas | Pasirinktą išteklių galima pakeisti iš kito failo įkeltu ištekliumi | 7 |

**Būsimo prototipo veikimo scenarijus:** Naudotojas įkelia žaidimo failą į programą, pasirenka jame esantį 3D modelį, kurį nori pakeisti, ir importuoja kitą modelį. Išsaugojęs pakeistą žaidimo failą, naudotojas paleidžia žaidimą ir patikrina, ar jame matomas naujasis modelis.

| Rizika arba neaiškumas | Kaip patikrinsiu arba sumažinsiu |
|---|---|
| Žaidimo Unity versija gali būti nurodyta ne iš karto matomoje failo vietoje. | Patikrinsiu žaidimo failus, kuriuose, kaip tikimasi, turi būti nurodyta versija, nes žaidimui jos reikia veikti. |
| Kai kuriuose žaidimuose failų laukai gali būti išdėstyti neįprastai. | Bandysiu programą su kuo daugiau žaidimų. Taip pat sieksiu, kad programa galėtų automatiškai prisitaikyti prie kai kurių formatų. Nepalaikomų žaidimų ar jų išteklių formatų atveju naudotojai galės pasiūlyti, ką reikėtų papildyti. |

## Šaltiniai
