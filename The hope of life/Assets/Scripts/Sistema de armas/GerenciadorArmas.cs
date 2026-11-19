
using System.Collections.Generic;
using UnityEngine;

public static class GerenciadorArmas
{
    private static readonly HashSet<DataArma> armasDesbloqueadas =
        new HashSet<DataArma>();

    public static bool EstaDesbloqueada(DataArma arma)
    {
        if (arma == null)
            return false;

        return arma.desbloqueadaInicialmente ||
               armasDesbloqueadas.Contains(arma);
    }

    public static void Desbloquear(DataArma arma)
    {
        if (arma == null)
            return;

        armasDesbloqueadas.Add(arma);
    }

    public static void Desbloquear(DataArma[] armas)
    {
        if (armas == null)
            return;

        foreach (DataArma arma in armas)
            Desbloquear(arma);
    }

    public static DataArma[] CapturarDesbloqueadas()
    {
        DataArma[] snapshot = new DataArma[armasDesbloqueadas.Count];
        armasDesbloqueadas.CopyTo(snapshot);
        return snapshot;
    }

    public static void RestaurarDesbloqueadas(DataArma[] snapshot)
    {
        armasDesbloqueadas.Clear();
        Desbloquear(snapshot);
    }
}
