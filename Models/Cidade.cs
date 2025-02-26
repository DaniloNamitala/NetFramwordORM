using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace DataSource.Models
{
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
}
