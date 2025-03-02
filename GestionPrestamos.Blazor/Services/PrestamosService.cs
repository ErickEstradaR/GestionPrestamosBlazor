using GestionPrestamos.Context;
using GestionPrestamos.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GestionPrestamos.Services;

public class PrestamosService(IDbContextFactory<Contexto> DbFactory)
{
    private async Task<bool> Existe(int prestamoId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Prestamos
            .AnyAsync(p => p.PrestamoId == prestamoId);
    }

    private async Task<bool> Insertar(Prestamos prestamo)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.Prestamos.Add(prestamo);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(Prestamos prestamo)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        var cuotasExistentes = contexto.PrestamosDetalle.Where(p => p.PrestamoId == prestamo.PrestamoId).ToList();
        contexto.PrestamosDetalle.RemoveRange(cuotasExistentes);
        contexto.Prestamos.Update(prestamo);
        
        var resultado = await contexto.SaveChangesAsync() > 0;

        return resultado;
    }


    public async Task<bool> Guardar(Prestamos prestamo )
    
    {
        
        if (prestamo.PrestamosDetalle == null || !prestamo.PrestamosDetalle.Any())
        {
            return false;  
        }
        prestamo.Balance = prestamo.Monto;
        if (!await Existe(prestamo.PrestamoId))
        {
            return await Insertar(prestamo);
        }
        else
        {
            return await Modificar(prestamo);
        }
    }

    public async Task<Prestamos?> Buscar(int prestamoId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Prestamos
            .Include(d => d.Deudor)
            .Where(p => p.Balance > 0) // Solo devuelve préstamos con balance pendiente
            .FirstOrDefaultAsync(p => p.PrestamoId == prestamoId);
    }


    public async Task<bool> Eliminar(int prestamoId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Prestamos
            .Where(p => p.PrestamoId == prestamoId)
            .ExecuteDeleteAsync() > 0;
    }

    public async Task<List<Prestamos>> GetList(Expression<Func<Prestamos, bool>> criterio)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Prestamos
            .Include(d => d.Deudor)
            .Where(criterio)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<List<Prestamos>> GetPrestamosPendientes(int deudorId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Prestamos
            .Where(p => p.DeudorId == deudorId && p.Balance > 0)
            .OrderBy(p => p.PrestamoId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Prestamos?> BuscarPrestamo(int id)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Prestamos.
            Include(p => p.Deudor)
            .FirstOrDefaultAsync(p => p.DeudorId == id);
    }
    
    public async Task<List<PrestamosDetalle>> ObtenerCuotas(int prestamoId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.PrestamosDetalle
            .Where(c => c.PrestamoId == prestamoId)
            .OrderBy(c => c.CuotaNo)
            .AsNoTracking()
            .ToListAsync();
    }
    
/*public async Task<bool> ActualizarCuotas(int prestamoId, List<PrestamosDetalle> nuevasCuotas)
{
    await using var contexto = await DbFactory.CreateDbContextAsync();
    var cuotasExistentes = await contexto.PrestamosDetalle
        .Where(c => c.PrestamoId == prestamoId)
        .ToListAsync();
    
        contexto.PrestamosDetalle.RemoveRange(cuotasExistentes);
    
    foreach (var cuota in nuevasCuotas)
    {
        var cuotaExistente = cuotasExistentes.FirstOrDefault(c => c.CuotaNo == cuota.CuotaNo);

        if (cuotaExistente == null)
        {
         
            contexto.PrestamosDetalle.Add(cuota);
        }
        else
        {
         
            cuotaExistente.Fecha = cuota.Fecha;
            cuotaExistente.Valor = cuota.Valor;
            cuotaExistente.Balance = cuota.Balance;
            contexto.PrestamosDetalle.Update(cuotaExistente);
        }
    }
    
    return await contexto.SaveChangesAsync() > 0;
    
}*/


}
