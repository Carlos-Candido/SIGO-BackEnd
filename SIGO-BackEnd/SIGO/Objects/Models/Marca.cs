using System.ComponentModel.DataAnnotations.Schema;

namespace SIGO.Objects.Models
{
    [Table("marca")]
    public class Marca
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("nome")]
        public string Nome { get; set; }

        [Column("desc")]
        public string Desc { get; set; }

        [Column("tipomarca")]
        public string TipoMarca { get; set; }

        public ICollection<Peca> Pecas { get; set; } = new List<Peca>();

        public Marca() { }

        public Marca(int id, string nome, string desc, string tipoMarca)
        {
            Id = id;
            Nome = nome;
            Desc = desc;
            TipoMarca = tipoMarca;
        }
    }
}