
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
}
