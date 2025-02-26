## Objetivo

O objetivo desse repositorio era desenvolver um ORM mínimo para acesso ao banco de dados em .Net Framework.

## Uso

### Model

1. Declarar a Model com os campos e tipos necessarios.
2. Anotar a classe com o nome da tabela correspondente no banco usando o [TableAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.schema.tableattribute?view=net-9.0)
3. Anotar as propriedades com o nome das colunas correspondentes usando o [ColumnAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.schema.ColumnAttribute?view=net-9.0)
4. Anotar a coluna de chave com o atributo [KeyAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.keyattribute?view=net-9.0)
5. Herdar a classe Entity (Opcional)

obs. A classe Entity serve apenas para prover metodos auxiliares e extensões para as models.

Exemplo:
```C#
[Table("CIDADES")]
public class Cidade : Entity
{
    [Column("ID"), Key]
    public int Id { get; set; }
    [Column("NOME")]
    public string Nome { get; set; }
    [Column("UF")]
    public string Uf { get; set; }
    [Column("CEP")]
    public string Cep { get; set; }
    [Column("AREA")]
    public double? Area { get; set; }
    [Column("ATIVO")]
    public bool Ativo { get; set; }
}
```

### Select

1. Usar o metodo `DatabaseConnection.Select`
2. Condições são passadas por parametro como uma Expression

Expressões binarias suportadas:
* \<
* \>
* \>=
* \<=
* ==
* &&
* ||

Funções Suportadas:
* String.StartsWith
* String.EndsWith
* String.Contains

```C#
// SELECT * FROM CIDADE
List<Cidade> result = DatabaseConnection.Instance.Select<Cidade>();

// SELECT * FROM CIDADES WHERE NOME = 'New York'
List<Cidade> result = DatabaseConnection.Instance.Select<Cidade>(c => c.Nome == "New York");

// SELECT * FROM CIDADES WHERE NOME LIKE '%Mos%' AND AREA > 1000
List<Cidade> result = DatabaseConnection.Instance.Select<Cidade>(c => c.Nome.Contains("Mos") && c.Area > 1000);
```

### Update

1. Usar o metodo `DatabaseConnection.Update` ou `Entity.Save`
2. O registro no banco vai ser atualizado usando a propriedade Key como referencia

obs. Todos os campos do model serão atualizados no banco 

```C#
Cidade city = DatabaseConnection.Instance.Select<Cidade>().First();
city.Nome = "New York"

// UPADATE CIDADE SET NOME = @NOME, AREA = @AREA ... WHERE ID = @ID
DatabaseConnection.Instance.Update(city);

// UPADATE CIDADE SET NOME = @NOME, AREA = @AREA ... WHERE ID = @ID
// Implicity calls DatabaseConnection.Update
city.Save()
```

### Delete

1. Usar o método `DatabaseConnection.Delete`
2. O registro no banco vai ser deletado usando a propriedade Key como referencia

```C#
Cidade city = DatabaseConnection.Instance.Select<Cidade>().First();

// DELETE FROM CIDADE WHERE ID == ID = @ID
DatabaseConnection.Instance.Delete(city);
```

### Insert

1. Usar o método `DatabaseConnection.Insert`

obs. Parametro passdo como referencia, vai atualizar a chave em caso de chaves autoincrement
```C#
Cidade city = new Cidade();
city.Nome = "Rome"

DatabaseConnection.Instance.Insert(ref city);
Console.WriteLine($"Inserted with ID {city.Id}");
```
