# DataPOProviderSQL - Guide d'Utilisation

## 📚 Vue d'Ensemble

Le système `DataPOProvider` propose deux approches complémentaires pour manipuler les objets DataPO en base de données :

1. **Provider Typé** (`DataPOProviderSQL<TPo>`) : Type-safe strict, limité à un seul type de PO
2. **Provider Générique** (`DataPOProviderSQL`) : Multi-types avec méthodes génériques

---

## 🎯 Provider Typé : Type-Safe Strict

### Utilisation Recommandée
- **Repository Pattern** : Services métier, couche d'accès aux données
- **Type-Safety** : Garantie au niveau du compilateur
- **Cohérence** : Un provider = Un type de PO

### Constructeurs

```csharp
// Option 1: Avec IDataConnector directement
var userProvider = new DataPOProviderSQL<UserPO>(connector);

// Option 2: Avec IMasterEnv (utilise automatiquement ConnectorDatabase)
var userProvider = new DataPOProviderSQL<UserPO>(env);
```

### Exemple

```csharp
// Créer un provider dédié aux UserPO
var userProvider = new DataPOProviderSQL<UserPO>(connector);

// ✅ LECTURE - Retourne uniquement des UserPO
var user = await userProvider.GetPOAsync(123);
var users = await userProvider.QueryPOAsync(100);

// ✅ ÉCRITURE - Accepte uniquement des UserPO
await userProvider.InsertPOAsync(newUser);
await userProvider.SavePOAsync(updatedUser);
await userProvider.DeletePOAsync(oldUser);

// ❌ IMPOSSIBLE - Pas de méthodes génériques GetPOAsync<T>
// var product = await userProvider.GetPOAsync<ProductPO>(456); // Erreur compilation !
```

### Implémentation Repository Pattern

```csharp
public class UserRepository
{
    private readonly DataPOProviderSQL<UserPO> _provider;
    
    public UserRepository(IDataConnector connector)
    {
        _provider = new DataPOProviderSQL<UserPO>(connector);
    }
    
    public Task<UserPO> GetByIdAsync(long id) 
        => _provider.GetPOAsync(id);
    
    public Task<CollectionPO<UserPO>> SearchByEmailAsync(string email)
    {
        var query = new QueryContext(
            "SELECT * FROM users WHERE email = @email",
            new Dictionary<string, object> { ["email"] = email }
        );
        return _provider.QueryPOAsync(query);
    }
    
    public Task CreateAsync(UserPO user) 
        => _provider.InsertPOAsync(user);
    
    public Task UpdateAsync(UserPO user) 
        => _provider.SavePOAsync(user);
    
    public Task DeleteAsync(UserPO user) 
        => _provider.DeletePOAsync(user);
}
```

---

## 🌐 Provider Générique : Multi-Types

### Utilisation Recommandée
- **Scripts de migration** : Manipulation de plusieurs tables
- **Outils génériques** : Import/Export, synchronisation
- **Prototypes rapides** : Tests, démonstrations

### Constructeurs

```csharp
// Option 1: Avec IDataConnector directement
var provider = new DataPOProviderSQL(connector);

// Option 2: Avec IMasterEnv (utilise automatiquement ConnectorDatabase)
var provider = new DataPOProviderSQL(env);
```

### Exemple

```csharp
// ✅ UN SEUL provider pour TOUS les types de PO
var provider = new DataPOProviderSQL(connector);

// Manipuler différents types de PO avec le même provider
var user = await provider.GetPOAsync<UserPO>(123);
var product = await provider.GetPOAsync<ProductPO>(456);
var order = await provider.GetPOAsync<OrderPO>(789);

// CRUD avec types différents
await provider.InsertPOAsync(newUser, newUser2);
await provider.InsertPOAsync(newProduct);
await provider.SavePOAsync(updatedOrder);
await provider.DeletePOAsync(oldUser);
```

### Script de Migration Exemple

```csharp
public class DataMigrationTool
{
    private readonly DataPOProviderSQL _provider;
    
    public DataMigrationTool(IDataConnector connector)
    {
        _provider = new DataPOProviderSQL(connector);
    }
    
    public async Task MigrateAllDataAsync()
    {
        // Migrer les utilisateurs
        var users = await _provider.QueryPOAsync<UserPO>(
            new QueryContext("SELECT * FROM users WHERE migrated = 0", null));
        
        foreach (var user in users)
        {
            user["migrated"] = 1;
            await _provider.SavePOAsync(user);
        }
        
        // Migrer les produits
        var products = await _provider.QueryPOAsync<ProductPO>(
            new QueryContext("SELECT * FROM products WHERE migrated = 0", null));
        
        foreach (var product in products)
        {
            product["migrated"] = 1;
            await _provider.SavePOAsync(product);
        }
        
        Console.WriteLine($"Migration terminée : {users.Count} users, {products.Count} products");
    }
}
```

---

## 📊 Comparaison des Approches

| Critère | Provider Typé `<TPo>` | Provider Générique |
|---------|----------------------|-------------------|
| **Type-Safety** | ✅ Strict (compile-time) | ⚠️ Responsabilité du développeur |
| **Usage** | Repository, Services métier | Scripts, Outils génériques |
| **Syntaxe** | `.GetPOAsync(id)` | `.GetPOAsync<UserPO>(id)` |
| **Flexibilité** | ❌ Un seul type | ✅ Multi-types |
| **Performance** | ✅ Optimale | ✅ Quasi-identique |
| **Maintenance** | ✅ Facile (IntelliSense complet) | ⚠️ Doit connaître les types |

---

## 🔧 API Complète du Provider Générique

### Méthodes de Lecture

```csharp
// Par ID auto-incrémenté
Task<TPo> GetPOAsync<TPo>(long idIncrement, int TenantId = 0)

// Par clé B36 (format Nglib)
Task<TPo> GetPOByKeyAsync<TPo>(string fullB36Key)

// Par dictionnaire de paramètres
Task<TPo> GetPOAsync<TPo>(Dictionary<string, object> paramKeys)

// Vérifier existence
Task<bool> ExistAsync<TPo>(Dictionary<string, object> keys)

// Requête avec QueryContext
Task<CollectionPO<TPo>> QueryPOAsync<TPo>(QueryContext query)
Task<TCollection> QueryPOAsync<TPo, TCollection>(QueryContext query)

// Requête avec limite et paramètres
Task<CollectionPO<TPo>> QueryPOAsync<TPo>(int limitResult = 1000, Dictionary<string, object> SqlParams = null)

// Recherche avec formulaire
Task<CollectionPO<TPo>> SearchPOAsync<TPo>(ISearchForm form, ITenant2 tenant = null)
```

### Méthodes d'Écriture

```csharp
// Insertion (INSERT)
Task InsertPOAsync<TPo>(params TPo[] items)

// Mise à jour (UPDATE)
Task<bool> SavePOAsync<TPo>(params TPo[] items)
Task<bool> SavePOAsync<TPo>(TPo[] items, bool ForceEvenIfNotModified = false)

// Mise à jour batch avec valeurs communes
Task UpdatePOAsync<TPo>(TPo[] items, Dictionary<string, object> valeursParameters)

// Suppression (DELETE)
Task DeletePOAsync<TPo>(params TPo[] items)
```

---

## 💡 Bonnes Pratiques

### ✅ À Faire

```csharp
// ✅ Repository Pattern avec provider typé
public class UserRepository
{
    private readonly DataPOProviderSQL<UserPO> _provider;
}

// ✅ Script générique avec provider multi-types
public class ExportTool
{
    private readonly DataPOProviderSQL _provider;
}

// ✅ Utiliser le bon provider selon le contexte
var typedProvider = new DataPOProviderSQL<UserPO>(connector);  // Pour UserRepository
var genericProvider = new DataPOProviderSQL(connector);        // Pour MigrationTool
```

### ❌ À Éviter

```csharp
// ❌ Utiliser le provider générique dans un repository dédié
public class UserRepository
{
    private readonly DataPOProviderSQL _provider; // ❌ Perte de type-safety
    
    public Task<UserPO> GetUserAsync(long id)
    {
        return _provider.GetPOAsync<UserPO>(id); // ⚠️ Risque d'erreur de type
    }
}

// ❌ Créer plusieurs providers typés quand un générique suffit
var userProvider = new DataPOProviderSQL<UserPO>(connector);
var productProvider = new DataPOProviderSQL<ProductPO>(connector);
var orderProvider = new DataPOProviderSQL<OrderPO>(connector);
// ✅ Mieux : var provider = new DataPOProviderSQL(connector);
```

---

## 🚀 Exemples Avancés

### Exemple 1 : API Controller avec Mapping

```csharp
public class UserController : Controller
{
    private readonly DataPOProvider<UserPO, UserModel> _provider;
    
    public UserController(IDataConnector connector)
    {
        _provider = new DataPOProvider<UserPO, UserModel>(connector);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(long id)
    {
        var userModel = await _provider.GetModelAsync(id);
        return userModel != null ? Ok(userModel) : NotFound();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserModel model)
    {
        var created = await _provider.InsertModelAsync(model);
        return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
    }
}
```

### Exemple 2 : Service de Synchronisation Multi-Tables

```csharp
public class SyncService
{
    private readonly DataPOProviderSQL _provider;
    
    public SyncService(IDataConnector connector)
    {
        _provider = new DataPOProviderSQL(connector);
    }
    
    public async Task SyncAllTablesAsync()
    {
        await SyncTableAsync<UserPO>("SELECT * FROM users WHERE sync_status = 0");
        await SyncTableAsync<ProductPO>("SELECT * FROM products WHERE sync_status = 0");
        await SyncTableAsync<OrderPO>("SELECT * FROM orders WHERE sync_status = 0");
    }
    
    private async Task SyncTableAsync<TPo>(string sql) where TPo : DataPO, new()
    {
        var items = await _provider.QueryPOAsync<TPo>(new QueryContext(sql, null));
        
        foreach (var item in items)
        {
            // Logique de synchronisation
            item["sync_status"] = 1;
            item["sync_date"] = DateTime.UtcNow;
        }
        
        await _provider.SavePOAsync(items.ToArray());
    }
}
```

### Exemple 3 : Transaction Multi-Types

```csharp
public async Task CreateOrderWithUserAsync(UserPO user, OrderPO order)
{
    var provider = new DataPOProviderSQL(connector);
    
    connector.BeginTransaction();
    try
    {
        // Insérer l'utilisateur
        await provider.InsertPOAsync(user);
        
        // Lier la commande à l'utilisateur
        order["userid"] = user.UserId;
        await provider.InsertPOAsync(order);
        
        connector.CommitTransaction();
    }
    catch
    {
        connector.RollBackTransaction();
        throw;
    }
}
```

---

## 📖 Conclusion

**Choisir le bon provider selon le contexte :**

- **Provider Typé** (`DataPOProviderSQL<TPo>`) → Repositories, Services métier, Type-safety strict
- **Provider Générique** (`DataPOProviderSQL`) → Scripts, Migrations, Outils génériques

Les deux approches coexistent harmonieusement et répondent à des besoins différents. 🎯
