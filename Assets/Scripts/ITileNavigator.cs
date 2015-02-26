using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface ITileNavigator
{
    Tile Left(int index);
    Tile Right(int index);
    Tile Top(int index);
    Tile Bottom(int index);
}
