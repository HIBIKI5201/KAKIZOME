namespace Master.Utility
{
    public static class ParticleAffiliationUtility
    {
        public static int GetAffiliationByIndex(int index, int currentPhase)
        {
            switch (index % 3)
            {
                case 0: return 3;
                case 1: return 4;
            }

            return currentPhase;
        }
    }
}
