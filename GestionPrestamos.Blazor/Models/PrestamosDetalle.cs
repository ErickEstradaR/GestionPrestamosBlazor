using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionPrestamos.Models;

public partial class PrestamosDetalle
{
    [Key]
    public int DetalleId { get; set; }

    public int PrestamoId { get; set; }  

    public int CuotaNo { get; set; } 

    public DateTime Fecha { get; set; } 

    public double Valor { get; set; } 
    
    // Campo para rastrear el balance pendiente de la cuota
    public double Balance { get; set; }
    
    // Campo para rastrear si la cuota está pagada
    public bool Pagada { get; set; }
    
    [ForeignKey("PrestamoId")]
    [InverseProperty("PrestamosDetalle")]
    public virtual Prestamos Prestamo { get; set; } = null!;
}