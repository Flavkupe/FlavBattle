using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DropUnitEventArgs : EventArgs
{
    public DraggableUIUnit Unit { get; set; }

    public FormationPair StartingPos { get; set; }

    public FormationPair EndingPos { get; set; }

    public DraggableUIUnit ReplacedUnit { get; set; }

    public IArmy Army { get; set; }
}

