using GestionPrestamos.Context;
using GestionPrestamos.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GestionPrestamos.Services;

//IDbContextFactory<Contexto> DbFactory
public class CobrosService(IDbContextFactory<Contexto> DbFactory)
{
    private async Task<bool> Existe(int cobroId)
    {
        await using var contexto= await DbFactory.CreateDbContextAsync();
        return await contexto.Cobros.AnyAsync(c => c.CobroId == cobroId);
    }

    private async Task<bool> Insertar(Cobros cobro)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.Cobros.Add(cobro);
        await AfectarPrestamos(cobro.CobrosDetalle.ToArray(), TipoOperacion.Resta);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task AfectarPrestamos(CobrosDetalle[] detalle, TipoOperacion tipoOperacion)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        foreach (var item in detalle)
        {
            var prestamo = await contexto.Prestamos.SingleAsync(p => p.PrestamoId == item.PrestamoId);
            if (tipoOperacion == TipoOperacion.Resta)
                prestamo.Balance -= item.ValorCobrado;
            else
                prestamo.Balance += item.ValorCobrado;
        }
        await contexto.SaveChangesAsync();
    }

    private async Task<bool> Modificar(Cobros cobro)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.Update(cobro);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Guardar(Cobros cobro)
    {
        if (!await Existe(cobro.CobroId))
        {
            return await Insertar(cobro);
        }
        else
        {
            return await Modificar(cobro);
        }
    }

    public async Task<Cobros> Buscar(int cobroId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Cobros.Include(d => d.Deudor)
            .Include(d => d.CobrosDetalle)
            .FirstOrDefaultAsync(c => c.CobroId == cobroId);
    }

    public async Task<bool> Eliminar(int cobroId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        var cobro = await contexto.Cobros
            .Include(c => c.CobrosDetalle)
            .FirstOrDefaultAsync(c => c.CobroId == cobroId);

        if (cobro == null) return false;

        await AfectarPrestamos(cobro.CobrosDetalle.ToArray(), TipoOperacion.Suma);

        contexto.CobrosDetalle.RemoveRange(cobro.CobrosDetalle);
        contexto.Cobros.Remove(cobro);
        var cantidad = await contexto.SaveChangesAsync();
        return cantidad > 0;
    }

    public async Task<List<Cobros>> Listar(Expression<Func<Cobros, bool>> criterio)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Cobros.Include(d => d.Deudor)
            .Include(d => d.CobrosDetalle)
            .Where(criterio)
            .AsNoTracking()
            .ToListAsync();
    }


}

public enum TipoOperacion
{
    Suma = 1,
    Resta = 2
}