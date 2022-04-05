----------------------------------------------------
Spolocne
----------------------------------------------------

----------------
Client.cs
----------------

- funkcia void RememberData(string[] words)
	- uklada slova do internej cache, ktora ked sa naplni (do 300 znakov spolu so zaciatkom a koncom prikazu), tak sa automaticky flushne - zavola sa FlushCache - ktora vola ExecuteCommand
- funkcia void FlushCache()
	- je volana z funkcie RememberData ked treba
	- moze byt volana z vonku po dokonceni naplnania dat, aby sa zvysne slova poslali cez ExecuteCommand, aj ked sa cache este nenaplnila
- funkcia IEnumerator WaitForPendingDataCoroutine()
	- pouziva sa na cakanie kym budu vsetky data spracovane (tym ze sa SuperDuperRememberer dotazuje na stav commandov)

cize ukazkove volanie je napr (vid funkciu ShouldRememberItems v Client.spec.cs):

	string[] words = new [] { "banana", "orange", "pomegranate" };
	client.RememberData(words);
	client.FlushCache();
	yield return client.WaitForPendingDataCoroutine();
	Assert.AreEqual(new string[] {"banana", "orange", "pomegranate"}, rememberer.Items);

funkcionalitu FlushCache som vytvaral kvoli optimalizacii SuperDuperRememberer , aby sa volala funkcia ExecuteCommand co najmenej casto

funkcionalitu WaitForPendingDataCoroutine som vytvaral aby bolo mozne cakat z unit testov na dokoncenie commandov a po ukonceni commandov bolo mozne porovnat vysledy

Clienta som nevytvaral ako thread-safe, predbezne predpokladam ze bude pouzivany v Unity a iba z main threadu. Coroutines su povolene (vyuzite pri funkcii WaitForPendingDataCoroutine)

Ak by bolo treba aby bol thread safe tak to samozrejme neni problem dorobit.

----------------
Client.spec.cs
----------------

upravil som funkciu ShouldRememberItems
- jednak aby volala spravne funkcie z clienta (vid vyssie, alebo v kode)
- jednak som ju spravil ako coroutinu, aby bolo mozne cakat na vysledky dobehnutia commandov v SuperDuperRememberer 

pridal som funckiu ShouldRememberManyLargeItems
- ktora sa snazi ulozit 100 slov, kazde dlzky 100 znakov, s tym ze raz zavola funkciu client.RememberData
- potom overuje ci su vsetky ulozene

pridal som funckiu ShouldRememberManyManySmallItems
- ktora sa snazi ulozit 100 slov, kazde dlzky 7 znakov, a toto cele opakuje 10 krat (10 krat zavola funkciu client.RememberData)
- potom overuje ci su vsetky ulozene

pridal som funkciu ShouldThrowExceptionOnWrongCharacters
- ktora testuje, ci sa vygeneruje exception ak sa pouzije nepovoleny znak v slove

pridal som funkciu ShouldThrowExceptionOnWordTooLong
- ktora testuje, ci sa vygeneruje exception ak sa pouzije slovo ktore presahuje povolenu dlzku (dlzka uvodu commandu + dlzka slova v uvodzovkach + dlzka zaveru commandu presiahnu povolenych 300 znakov)

----------------------------------------------------
Varianty
----------------------------------------------------

v SuperDuperRememberer som narazil na dva nedostatky
- na generovanie id pouziva randomGenerator.Next a zda sa mi ze nieje uplne zarucene ze id bude unikatne
- pouziva Task.Run, a z takto vzniknutych thredov pristupuje ku rememberedItems - pri vatsom zatazeni sa to prejavuje nespravnymi hodnotami v tomto liste

co sa tyka generovania id, je to mozne riesit iba zmenou generovania id - navrhol som funkciu GenerateID, ktora pouziva HashSet aby overila ci je id unikatne (v ramci jednej instancie SuperDuperRememberer)

co sa tyka nepripravenosti rememberedItems na multi-threading, ako najjednoduchsie je pridat lock pri pristupe ku tomuto listu - navrhol som to priamo do SuperDuperRememberer 

zmeny v SuperDuperRememberer su v zadani zakazane, cize ak by som sa im snazil vyhnut, tak:
- GenerateID by som asi nevedel spravit bez zasahu do SuperDuperRememberer , kedze sa id generuje cisto interne v nom
- namiesto locku pre pristup ku rememberedItems by som spravil samostatny thread v Clientovi, kde by som vzdy dovolil len jeden command a kym by nebol dokonceny tak by som dalsi nevolal
	- zaroven by som si ukladal do zoznamu slova / commandy ktore potrebujem este poslat
	- toto som uz neimplementoval, ale samozrejme mozem

