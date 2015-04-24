using Newtonsoft.Json;
using System.Collections.Generic;


namespace twbot
{
    /// <summary>Information about a village</summary>
    public class VillageData
    {
        /// <summary>Village ID</summary>
        public short id;

        /// <summary>Name of the village</summary>
        public string name;

        /// <summary>X-coordinate</summary>
        public short coord_x;

        /// <summary>Y-coordinate</summary>
        public short coord_y;

        /// <summary>Class holding building data</summary>
        public BuildingData buildings;

        /// <summary>Class holding units</summary>
        public UnitsData units;

        /// <summary>Class holding Resource information (wood/stone/iron/storage_max)</summary>
        public Resources res;

        /// <summary>
        /// Converts the class to a json string
        /// </summary>
        ///	<returns>json string</returns>
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

    }

    /// <summary>Stores resources and storage capacity</summary>
    public class Resources
    {
        /// <summary>Stores wood/stone/iron resources</summary>
        public Dictionary<string, int> resources;

        /// <summary>Maximum Storage capacity</summary>
        public int storage_max;

        /// <summary>
        /// Converts the class to a JSON String
        /// </summary>
        ///	<returns>JSON String</returns>
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// returns the integer value of a resource
        /// <seealso cref="resources" />
        /// </summary>
        /// <param name="type">Resource-Type: wood/stone/iron </param>
        ///	<returns>Integer value of the current value on the resource</returns>
        public int getResource(string type)
        {
            if ( !resources.ContainsKey(type) )
                return 0;
            return resources[type];
        }
    }


    /// <summary>Stores level of buildings and overall stage</summary>
    public class BuildingData
    {
        /// <summary>Headquarters building level</summary>
        public short building_main;

        /// <summary>Barracks building level</summary>
        public short building_barracks;

        /// <summary>Stable building level</summary>
        public short building_stable;

        /// <summary>Garage building level</summary>
        public short building_garage;

        /// <summary>Snob building level</summary>
        public short building_snob;

        /// <summary>Smith building level</summary>
        public short building_smith;

        /// <summary>Place building level</summary>
        public short building_place;

        /// <summary>Market building level</summary>
        public short building_market;

        /// <summary>Timber camp level</summary>
        public short building_wood;

        /// <summary>Clay pit level</summary>
        public short building_stone;

        /// <summary>Iron mine level</summary>
        public short building_iron;

        /// <summary>Farm building level</summary>
        public short building_farm;

        /// <summary>Warehouse building level</summary>
        public short building_storage;

        /// <summary>Hiding place level</summary>
        public short building_hide;

        /// <summary>Wall level</summary>
        public short building_wall;

        /// <summary>current stage of the village</summary>
        public int level;


        /// <summary>
        /// sets a building to a new level
        /// </summary>
        /// <param name="building">Name of the affected building</param>
        /// <param name="level">New level</param>
        public void set(string building, short level)
        {
            query(building, false, level);
        }

        /// <summary>
        /// gets the current level of a building
        /// </summary>
        /// <param name="building">the building to be queried</param>
        ///	<returns>level of the affected building</returns>
        public short get(string building)
        {
            return query(building, true);
        }

        /// <summary>
        /// converts the class to a JSON String
        /// </summary>
        ///	<returns>JSON String</returns>
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Method to set or get building levels
        /// </summary>
        /// <param name="building">affected building</param>
        /// <param name="op">Operation, true means get current level and false to set it</param>
        /// <param name="level">the new level (only used when op == false)</param>
        ///	<returns>the level of the affected building</returns>
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


    /// <summary>Stores amount of units in a village</summary>
    public class UnitsData
    {
        /// <summary>Amount of Unit 'spear'</summary>
        public short unit_spear;

        /// <summary>Amount of Unit 'sword'</summary>
        public short unit_sword;

        /// <summary>Amount of Unit 'axe'</summary>
        public short unit_axe;

        /// <summary>Amount of Unit 'archer'</summary>
        public short unit_archer;

        /// <summary>Amount of Unit 'spy'</summary>
        public short unit_spy;

        /// <summary>Amount of Unit 'light'</summary>
        public short unit_light;

        /// <summary>Amount of Unit 'marcher' (mounted archer)</summary>
        public short unit_marcher;

        /// <summary>Amount of Unit 'heavy'</summary>
        public short unit_heavy;

        /// <summary>Amount of Unit 'ram'</summary>
        public short unit_ram;

        /// <summary>Amount of Unit 'catapult'</summary>
        public short unit_catapult;

        /// <summary>Amount of Unit 'snob'</summary>
        public short unit_snob;


        /// <summary>
        /// returns the class as JSON String
        /// </summary>
        ///	<returns>JSON string</returns>
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }   

    }


}
