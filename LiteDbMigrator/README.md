# LiteDbMigrator: Come Funziona `Migrator.cs`

Il file `Migrator.cs` contiene una classe chiamata `Migrator` progettata per gestire le migrazioni di schema in un database LiteDB. Questa classe consente di rinominare collezioni, campi e applicare trasformazioni ai documenti o ai sotto-documenti. Di seguito viene fornita una spiegazione dettagliata del funzionamento.

---

## Struttura della Classe `Migrator`

### **Campi Privati**
- **`_db`**: Istanza del database LiteDB.
- **`_dbPath`**: Percorso del file del database (utilizzato se non viene fornita un'istanza di `LiteDatabase`).
- **`_collectionName`**: Nome della collezione su cui operare.
- **`_newCollectionName`**: Nome della nuova collezione (se si desidera rinominare una collezione).
- **`_migrations`**: Lista di azioni (trasformazioni) da applicare ai documenti della collezione.

### **Costruttori**
- **`Migrator(string dbPath)`**: Inizializza il migratore con il percorso del database.
- **`Migrator(LiteDatabase db)`**: Inizializza il migratore con un'istanza di `LiteDatabase`.

---

## Metodi Principali

### **1. Gestione delle Collezioni**
- **`Collection(string name)`**:
  Specifica il nome della collezione su cui operare.

- **`RenameCollection(string newCollectionName)`**:
  Imposta un nuovo nome per la collezione e prepara il migratore per rinominare la collezione.

- **`RenameCollectionInternal()`** *(privato)*:
  - Copia tutti i documenti dalla collezione originale a una nuova collezione.
  - Elimina la collezione originale.
  - Aggiorna `_collectionName` con il nuovo nome.

---

### **2. Migrazione dei Campi**
- **`RenameField(string oldName, string newName)`**:
  Aggiunge una trasformazione alla lista `_migrations` per rinominare un campo in ogni documento. Se il campo esiste:
  - Copia il valore nel nuovo campo.
  - Rimuove il campo originale.

---

### **3. Gestione dei Sotto-Documenti**
- **`ForEachInArray(string arrayField, Action<SubDocumentMigrator> config)`**:
  - Targetizza un campo array in ogni documento.
  - Itera su ogni sotto-documento nell'array e applica una trasformazione definita dall'utente utilizzando la classe `SubDocumentMigrator`.

---

### **4. Esecuzione delle Migrazioni**
- **`Execute()`**:
  - Se è stato specificato un nuovo nome per la collezione, chiama `RenameCollectionInternal()` per rinominare la collezione.
  - Recupera tutti i documenti della collezione.
  - Applica tutte le trasformazioni definite in `_migrations` a ciascun documento.
  - Aggiorna i documenti modificati nella collezione.

---

## Classe `SubDocumentMigrator`

La classe `SubDocumentMigrator` è un helper per gestire le trasformazioni sui sotto-documenti. Fornisce i seguenti metodi:

- **`RenameField(string oldName, string newName)`**:
  Rinomina un campo in un sotto-documento.

- **`ForEachInArray(string fieldName, Action<SubDocumentMigrator> action)`**:
  Itera su un array in un sotto-documento e applica trasformazioni a ciascun elemento.

---

## Esempio di Utilizzo

Ecco un esempio pratico di come utilizzare la classe `Migrator`:
```csharp
var migrator = new Migrator("myDatabase.db") 
	.Collection("Users") 
	.RenameCollection("NewUsers")
	.RenameField("oldField", "newField") 
	.ForEachInArray("addresses", sub => sub.RenameField("oldStreet", "newStreet"));

migrator.Execute();
``` 
 
### Cosa fa questo codice:
1. Specifica la collezione `Users` come target.
2. Rinomina il campo `oldField` in `newField` in ogni documento della collezione.
3. Per ogni documento, rinomina il campo `oldStreet` in `newStreet` all'interno dell'array `addresses`.
4. Applica tutte le modifiche al database.

---

## Vantaggi del Design

- **Composizione**: Le migrazioni possono essere definite in modo modulare e composito.
- **Flessibilità**: Supporta trasformazioni sia a livello di documento che di sotto-documento.
- **Sicurezza**: Le modifiche vengono applicate solo dopo aver eseguito `Execute()`.

---

## Conclusione

La classe `Migrator` è uno strumento potente per gestire le migrazioni di schema in LiteDB. Grazie alla sua API fluida e alla possibilità di definire trasformazioni personalizzate, consente di aggiornare facilmente la struttura dei dati senza interventi manuali.

