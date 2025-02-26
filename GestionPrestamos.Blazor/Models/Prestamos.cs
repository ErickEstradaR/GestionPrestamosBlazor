using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GestionPrestamos.Models;

public partial class Prestamos
{
    [Key]
    public int PrestamoId { get; set; }

    [Required(ErrorMessage = "Este campo es requerido")]
    public string Concepto { get; set; } = null!;

    [Range(1, double.MaxValue, ErrorMessage = "El monto no puede ser menor a 1")]
    public double Monto { get; set; }

    public double Balance { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un deudor válido")]
    public int DeudorId { get; set; }
    
    [ForeignKey("DeudorId")]
    [InverseProperty("Prestamos")]
    public virtual Deudores Deudor { get; set; } = null!;
    
    [InverseProperty("Prestamo")]
    public virtual ICollection<PrestamosDetalle> PrestamosDetalle { get; set; } = new List<PrestamosDetalle>();
}
