using Newtonsoft.Json;

namespace twbot
{
    public class VillageData
    {
        public short id;
        public string name;
        public short coord_x;
        public short coord_y;
        public BuildingData buildings;
        public UnitsData units;

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

    }

    public class BuildingData
    {
        public short building_main;
        public short building_barracks;
        public short building_stable;
        public short building_garage;
        public short building_snob;
        public short building_smith;
        public short building_place;
        public short building_market;
        public short building_wood;
        public short building_stone;
        public short building_iron;
        public short building_farm;
        public short building_storage;
        public short building_hide;
        public short building_wall;



        public void set(string building, short level)
        {
            query(building, false, level);
        }

        public short get(string building)
        {
            return query(building, true);
        }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        /*
         * Method to set or get building levels.
         * set op to true to get building level and false to set it
         *  returns the requested building level
         */
        private short query(string building, bool op, short level = 0)
        {
            switch (building)
            {
                case "main":
                    return (op ? building_main : building_main=level);

                case "barracks":
                    return (op ? building_barracks : building_barracks=level);

                case "stable":
                    return (op ? building_stable : building_stable=level);

                case "garage":
                    return (op ? building_garage : building_garage=level);

                case "snob":
                    return (op ? building_snob : building_snob=level);

                case "smith":
                    return (op ? building_smith : building_smith=level);

                case "place":
                    return (op ? building_place : building_place=level);

                case "market":
                    return (op ? building_market : building_market=level);

                case "wood":
                    return (op ? building_wood : building_wood=level);

                case "stone":
                    return (op ? building_stone : building_stone=level);

                case "iron":
                    return (op ? building_iron : building_iron=level);

                case "farm":
                    return (op ? building_farm : building_farm=level);

                case "storage":
                    return (op ? building_storage : building_storage=level);

                case "hide":
                    return (op ? building_hide : building_hide=level);

                case "wall":
                    return (op ? building_wall : building_wall=level);



            }

            return 0;
        }

    }

    public class UnitsData
    {
        public short unit_spear;
        public short unit_sword;
        public short unit_axe;
        public short unit_archer;
        public short unit_spy;
        public short unit_light;
        public short unit_marcher;
        public short unit_heavy;
        public short unit_ram;
        public short unit_catapult;
        public short unit_snob;

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }   

    }


}
