using System.Collections.Generic;
using static SpawnConfig.ListManager;

namespace SpawnConfig.ExtendedClasses;

public class ExtendedGroupCounts {

    public int level = 1;
    public List<List<int>> possibleGroupCounts = [];

    public ExtendedGroupCounts(int i){
        
        level = i + 1;
        possibleGroupCounts.Add([difficulty1Counts[i], difficulty2Counts[i], difficulty3Counts[i]]);
        
    }

}