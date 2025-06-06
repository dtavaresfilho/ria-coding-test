using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ria_coding_test_part2.Model
{
    public class CustomerStorage
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "jsonb")]
        public string Data { get; set; } = "{}";
    }
}
