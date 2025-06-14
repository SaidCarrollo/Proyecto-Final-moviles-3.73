
public enum ProjectilePowerType
{
    Normal,            // Sin poder especial, solo impacto
    ExplodeOnImpact,   // Explota al chocar con algo
    SplitOnTap,        // Se divide en varios proyectiles si el jugador toca la pantalla despu�s de lanzarlo
    SpeedBoostOnTap,   // Obtiene un impulso de velocidad si el jugador toca la pantalla
    PierceThrough,    // Atraviesa un n�mero limitado de objetos d�biles
    DropBomb,          // Lanza un "huevo" hacia abajo y el proyectil principal sube
    Homing //Va hacia dondee marques con el touch
}