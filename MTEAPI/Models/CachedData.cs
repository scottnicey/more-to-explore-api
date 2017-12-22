using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MTEAPI.Models
{
    public class DataAccessCachedData
    {
 
        public DataAccessCachedData()
        {
            //_appsettings = appsettings;
        }


        public CachedDataClass.cachedData Get()
        {
            CachedDataClass c = new CachedDataClass(null);
            return c.data;
        }

        public CachedDataClass.cachedData Get(AppSettings appsettings)
        {
            CachedDataClass c = new CachedDataClass(appsettings);
            return c.data;
        }
    }


    public class CachedDataClass
    {

        public cachedData data;
        private readonly AppSettings _appsettings;


        public CachedDataClass(AppSettings appsettings)
        {
            try
            {
                _appsettings = appsettings; 
                data = new cachedData();
                loadData();
            }
            catch (Exception ex)
            {
                throw new Exception("Error caching data: " + ex.ToString());  
            }
        }


        public class cachedData
        {
            public Track.RootObject  track { get; set; }
            public Site.RootObject site { get; set; }
            public Hut.RootObject hut { get; set; }
            public Asset.RootObject asset { get; set; }
            public Relic.RootObject relic { get; set; }
            public Carpark.RootObject carpark { get; set; }
            public string dataFrom { get; set; }
        }

        public void loadData()
        {
            string s_tracks = System.IO.File.ReadAllText(_appsettings.DataLocation +  "\\tracks.json");
            string s_sites = System.IO.File.ReadAllText(_appsettings.DataLocation + "\\sites.json");
            string s_huts = System.IO.File.ReadAllText(_appsettings.DataLocation + "\\huts.json");
            string s_carparks = System.IO.File.ReadAllText(_appsettings.DataLocation + "\\carparks.json");
            string s_relics = System.IO.File.ReadAllText(_appsettings.DataLocation + "\\relics.json");
            string s_assets = System.IO.File.ReadAllText(_appsettings.DataLocation + "\\assets.json");

            data.track = JsonConvert.DeserializeObject<Track.RootObject>(s_tracks);
            data.site = JsonConvert.DeserializeObject<Site.RootObject>(s_sites);
            data.hut = JsonConvert.DeserializeObject<Hut.RootObject>(s_huts);
            data.carpark = JsonConvert.DeserializeObject<Carpark.RootObject>(s_carparks);
            data.relic = JsonConvert.DeserializeObject<Relic.RootObject>(s_relics);
            data.asset = JsonConvert.DeserializeObject<Asset.RootObject>(s_assets);



            //#######################################################################
            //#######################################################################
            //#######################################################################
            //First set hierarchy for sites and carparks
            //#######################################################################
            //#######################################################################
            //#######################################################################
            foreach (Hut.Feature feh in data.hut.features)
            {
                foreach (Site.Feature fes in data.site.features)
                {
                    if (feh.properties.IS_PART_OF == fes.properties.SERIAL_NO)
                    {
                        feh.properties.IS_PART_OF_TRACK = fes.properties.IS_PART_OF;
                        break;
                    }
                }
            }

            foreach (Carpark.Feature fec in data.carpark.features)
            {
                foreach (Site.Feature fes in data.site.features)
                {
                    if (fec.properties.IS_PART_OF == fes.properties.SERIAL_NO)
                    {
                        fec.properties.IS_PART_OF_TRACK = fes.properties.IS_PART_OF;
                        break;
                    }
                }
            }

            foreach (Asset.Feature fea in data.asset.features)
            {
                foreach (Site.Feature fes in data.site.features)
                {
                    if (fea.properties.IS_PART_OF == fes.properties.SERIAL_NO)
                    {
                        fea.properties.IS_PART_OF_TRACK = fes.properties.IS_PART_OF;
                        break;
                    }
                }
            }


            //ASSETS ################################################
            //ASSETS ################################################
            //ASSETS ################################################
            //ASSETS ################################################
            //ASSETS ################################################
            //ASSETS ################################################
            foreach (Asset.Feature f in data.asset.features)
            {
                f.assetFeatures = new List<string>();
                f.filters = new Asset.Filters();
                f.filters.amenities = new List<string>();

                string ispartof = f.properties.IS_PART_OF;
                string serialno = f.properties.SERIAL_NO;  
                if (ncustr(f.properties.ASSET_CLS) == "INFORMATION SHELTER")
                {
                    f.assetFeatures.Add("informationShelter");
                    f.filters.amenities.Add("informationShelter");
                }
                if (ncustr(f.properties.ASSET_CLS) == "PICNIC SHELTER")
                {
                    f.assetFeatures.Add("picnicShelter");
                    f.filters.amenities.Add("picnicShelter");
                }
                if (ncustr(f.properties.ASSET_CLS) == "TOILET BLOCK")
                {
                    f.assetFeatures.Add("toiletBlock");
                    f.filters.amenities.Add("toiletBlock");
                }
                if (ncustr(f.properties.ASSET_CLS) == "VIEWING PLATFORM")
                {
                    f.assetFeatures.Add("viewingPlatform");
                    f.filters.amenities.Add("viewingPlatform");
                }

                //if (ncustr(f.properties.ASSET_CLS) == "SITE ELEMENT") f.assetFeatures.Add("siteElement");
                if (ncustr(f.properties.ASSET_CLS) == "BOARDWALK") f.assetFeatures.Add("boardwalk");
                if (ncustr(f.properties.ASSET_CLS) == "BRIDGE") f.assetFeatures.Add("bridge");
                if (ncustr(f.properties.ASSET_CLS) == "STAIRS") f.assetFeatures.Add("stairs");
                if (ncustr(f.properties.ASSET_CLS) == "JETTY") f.assetFeatures.Add("jetty");
            }




            //SITES ################################################
            //SITES ################################################
            //SITES ################################################
            //SITES ################################################
            //SITES ################################################
            //SITES ################################################
            foreach (Site.Feature f in data.site.features)
            {
                f.siteFeatures = new List<string>();
                f.filters = new Site.Filters();
                f.filters.access = new List<string>();
                f.filters.activities = new List<string>();
                f.filters.amenities = new List<string>();

                List<int> tempPhotos = new List<int>();


                //NOW GET ANY ASSETS THAT ARE A PART OF THE SITE
                foreach (Asset.Feature fea in data.asset.features)
                {
                    if (f.properties.SERIAL_NO == fea.properties.IS_PART_OF)
                    {
                        foreach (string af in fea.assetFeatures)
                        {
                            f.siteFeatures.Add(af);
                        }

                        foreach (string fil in fea.filters.amenities)
                        {
                            f.filters.amenities.Add(fil);
                        }

                        int pc1 = ncint(f.properties.PHOTO_ID_1);
                        int pc2 = ncint(f.properties.PHOTO_ID_2);
                        int pc3 = ncint(f.properties.PHOTO_ID_3);
                        if (pc1 > 0 && !tempPhotos.Contains(pc1)) tempPhotos.Add(pc1);
                        if (pc2 > 0 && !tempPhotos.Contains(pc2)) tempPhotos.Add(pc2);
                        if (pc3 > 0 && !tempPhotos.Contains(pc3)) tempPhotos.Add(pc3);
                    }
                }

                foreach (Hut.Feature feh in data.hut.features)
                {
                    if (f.properties.SERIAL_NO == feh.properties.IS_PART_OF)
                    {
                        f.siteFeatures.Add("hut");
                        f.filters.amenities.Add("hut");

                        int ph1 = ncint(f.properties.PHOTO_ID_1);
                        int ph2 = ncint(f.properties.PHOTO_ID_2);
                        int ph3 = ncint(f.properties.PHOTO_ID_3);
                        if (ph1 > 0 && !tempPhotos.Contains(ph1)) tempPhotos.Add(ph1);
                        if (ph2 > 0 && !tempPhotos.Contains(ph2)) tempPhotos.Add(ph2);
                        if (ph3 > 0 && !tempPhotos.Contains(ph3)) tempPhotos.Add(ph3);
                    }
                }
                foreach (Carpark.Feature fec in data.carpark.features)
                {
                    if (f.properties.SERIAL_NO == fec.properties.IS_PART_OF)
                    {
                        f.siteFeatures.Add("carpark");

                        int pc1 = ncint(f.properties.PHOTO_ID_1);
                        int pc2 = ncint(f.properties.PHOTO_ID_2);
                        int pc3 = ncint(f.properties.PHOTO_ID_3);
                        if (pc1 > 0 && !tempPhotos.Contains(pc1)) tempPhotos.Add(pc1);
                        if (pc2 > 0 && !tempPhotos.Contains(pc2)) tempPhotos.Add(pc2);
                        if (pc3 > 0 && !tempPhotos.Contains(pc3)) tempPhotos.Add(pc3);
                    }
                }


                /*
                foreach (Relic.Feature fec in data.relic.features)
                {
                    if (f.properties.SERIAL_NO == fec.properties.IS_PART_OF)
                    {
                        f.siteFeatures.Add("relic");

                        int pc1 = ncint(f.properties.PHOTO_ID_1);
                        int pc2 = ncint(f.properties.PHOTO_ID_2);
                        int pc3 = ncint(f.properties.PHOTO_ID_3);
                        if (pc1 > 0 && !tempPhotos.Contains(pc1)) tempPhotos.Add(pc1);
                        if (pc2 > 0 && !tempPhotos.Contains(pc2)) tempPhotos.Add(pc2);
                        if (pc3 > 0 && !tempPhotos.Contains(pc3)) tempPhotos.Add(pc3);
                    }
                }
                */


                bool camping = false, caravaning = false, campervanning = false, fishing = false, fossicking = false, hangGliding = false, trailBikeVisitorArea = false, treeviewing = false; 
                bool heritage = false, paddling = false, picnicing = false, rockclimbing = false, dogWalking = false, wildlife = false, bbq = false;


                if (ncustr(f.properties.TREE_VIEW) != "") { f.siteFeatures.Add("treeViewing"); treeviewing = true; }
                if (ncustr(f.properties.WILDERNESS) != "") f.siteFeatures.Add("wilderness");

                if (ncustr(f.properties.CAMPING) != "") { f.siteFeatures.Add("camping"); camping = true; }


                /*
                if (ncustr(f.properties.CARAVAN) == "CAMPERVANNING") { f.siteFeatures.Add("campervanning"); campervanning = true; }
                else
                if (ncustr(f.properties.CARAVAN) != "") {
                    f.siteFeatures.Add("caravaning"); caravaning = true;
                }
                */
                if (ncustr(f.properties.CAMPERVANNING) == "CAMPERVANNING")
                {
                    string ct = ncstr(f.properties.CAMPERVAN_TYPE).ToLower();
                    if (ct.Length > 0)
                    {
                        campervanning = true;
                        f.siteFeatures.Add("campervanning");
                        if (ct.Contains("trailer"))
                        {
                            f.siteFeatures.Add("camperTrailer");
                        }
                        else
                        if (ct.Contains("small"))
                        {
                            f.siteFeatures.Add("smallCaravan");
                        }
                        else
                        {
                            if (ct.Length > 0) f.siteFeatures.Add(ct);
                        }
                    }
                }









                if (ncustr(f.properties.FISHING) != "") { f.siteFeatures.Add("fishing"); fishing = true; }
                if (ncustr(f.properties.FOSSICKING) != "") { f.siteFeatures.Add("fossicking"); fossicking = true; }
                if (ncustr(f.properties.HANG_GLIDE) != "") { f.siteFeatures.Add("hangGliding"); hangGliding = true; }
                if (ncustr(f.properties.HERITAGE) != "") { f.siteFeatures.Add("heritage"); heritage = true; }
                if (ncustr(f.properties.PADDLING) != "") { f.siteFeatures.Add("paddling"); paddling = true; }
                if (ncustr(f.properties.PICNICING) != "") { f.siteFeatures.Add("picnicking"); picnicing = true; }
                if (ncustr(f.properties.ROCKCLIMB) != "") { f.siteFeatures.Add("rockclimbing"); rockclimbing = true; }
                if (ncustr(f.properties.WALKINGDOG) != "") { f.siteFeatures.Add("dogWalking"); dogWalking = true; }
                if (ncustr(f.properties.WILDLIFE) != "") { f.siteFeatures.Add("wildlife"); wildlife = true; }
                if (ncustr(f.properties.TB_VISITOR) != "") { f.siteFeatures.Add("trailBikeVisitorArea"); trailBikeVisitorArea = true; }

                if (ncustr(f.properties.BBQ_ELEC) != "") { f.siteFeatures.Add("bbqElectric"); bbq = true; }
                if (ncustr(f.properties.BBQ_GAS) != "") { f.siteFeatures.Add("bbqGas"); bbq = true; }
                if (ncustr(f.properties.BBQ_PIT) != "") { f.siteFeatures.Add("bbqPit"); bbq = true; }
                if (ncustr(f.properties.BBQ_WOOD) != "") { f.siteFeatures.Add("bbqWood"); bbq = true; }

                

                //START ACTIVITIES MUST BE ALPHABETIC ORDER
                f.activities = new List<Site.Activity>();
                if (camping)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "camping";
                    a.comment = f.properties.CAMPING_C;
                    f.activities.Add(a);
                    if (f.properties.IS_PART_OF != null)
                    {
                        int test = 1;
                    }
                }

                /*
                if (caravaning)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "caravaning";
                    a.comment = f.properties.CARAVAN_C;
                    f.activities.Add(a);
                    f.filters.access.Add(a.activityType);
                }
                */
                if (campervanning)
                {
                    if (ncustr(f.properties.CAMPERVAN_TYPE) != "")
                    {
                        string ct = f.properties.CAMPERVAN_TYPE.ToString().ToLower();
                        if (ct.Contains("trailer"))
                        {
                            Site.Activity a = new Site.Activity();
                            a.activityType = "camperTrailer";
                            a.comment = ncstr(f.properties.CAMPERVAN_C);
                            f.activities.Add(a);
                            f.filters.access.Add(a.activityType);
                        }
                        if (ct.Contains("small"))
                        {
                            Site.Activity a = new Site.Activity();
                            a.activityType = "smallCaravan";
                            a.comment = ncstr(f.properties.CAMPERVAN_C);
                            f.activities.Add(a);
                            f.filters.access.Add(a.activityType);
                        }
                    }
                }


                if (dogWalking)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "dogWalking";
                    a.comment = f.properties.WALKDOG_C;
                    f.activities.Add(a);
                }

                if (fishing)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "fishing";
                    a.comment = f.properties.FISHING_C;
                    f.activities.Add(a);
                }

                if (fossicking)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "fossicking";
                    a.comment = f.properties.FOSSICK_C;
                    f.activities.Add(a);
                    if(f.properties.IS_PART_OF != null )
                    {
                        int test = 1;
                    }
                }

                if (hangGliding)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "hangGliding";
                    a.comment = f.properties.HANG_GLD_C;
                    f.activities.Add(a);
                }

                if (heritage)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "heritage";
                    a.comment = f.properties.HERITAGE_C;
                    f.activities.Add(a);
                }
                if (paddling)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "paddling";
                    a.comment = f.properties.PADDLING_C;
                    f.activities.Add(a);
                }

                if (picnicing)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "picnicking";
                    a.comment = f.properties.PICNICING_C;
                    f.activities.Add(a);
                }

                if (rockclimbing)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "rockclimbing";
                    a.comment = f.properties.ROCKCLIMB_C;
                    f.activities.Add(a);
                }

                if (trailBikeVisitorArea)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "trailBikeVisitorArea";
                    a.comment = f.properties.TBVA_C;
                    f.activities.Add(a);
                }

                if (treeviewing)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "treeViewing";
                    a.comment = f.properties.TREEVIEW_C;
                    f.activities.Add(a);
                }

                if (wildlife)
                {
                    Site.Activity a = new Site.Activity();
                    a.activityType = "wildlife";
                    a.comment = f.properties.WILDLIFE_C;
                    f.activities.Add(a);
                }

                if(f.activities.Count > 0) f.siteDescriptrionFromFirstActivity = f.activities[0].comment;
                //END ACTIVITIES MUST BE ALPHABETIC ORDER


                //this is here because of ordering
                if (ncustr(f.properties.DIS_ACCESS) != "")
                {
                    Site.Activity a = new Site.Activity();
                    if (f.properties.DIS_ACCESS.ToUpper().Contains("PARTIAL"))
                    {
                        f.siteFeatures.Add("disabledAccessPartial");
                        a.activityType = "disabledAccessPartial";
                        a.comment = "Generally accessible subject to site constraints.";
                        f.activities.Add(a); 
                    }
                    else
                    if (f.properties.DIS_ACCESS.ToUpper().Contains("FULL"))
                    {

                        f.siteFeatures.Add("disabledAccessFull");
                        a.activityType = "disabledAccessFull";
                        a.comment = "Compliant with Australian Standards.";
                        f.activities.Add(a);
                    }
                    else
                    {
                        f.siteFeatures.Add("disabledAccess");
                        a.activityType = "disabledAccess";
                        f.activities.Add(a);

                    }

                    f.filters.access.Add("disabledAccess");
                }



                if (bbq)
                {
                    //Site.Activity a = new Site.Activity();
                    //a.activityType = "bbq";
                    //string b = "";
                    //if (f.properties.BBQ_ELEC != null) b = ncappend(b, "Electricty");
                    //if (f.properties.BBQ_GAS != null) b = ncappend(b, "Gas");
                    //if (f.properties.BBQ_WOOD != null) b = ncappend(b, "Wood");
                    //if (f.properties.BBQ_PIT != null) b = ncappend(b, "Pit");
                    //f.activities.Add(a);

                    f.filters.amenities.Add("bbq"); 
                }

                if (trailBikeVisitorArea)
                {
                    f.filters.amenities.Add("trailBikeVisitorArea");
                }



                List<string> siteActivityFilter  = new List<string>();
                foreach(Site.Activity a in f.activities)
                {
                    siteActivityFilter.Add(a.activityType);
                }
                f.filters.activities = siteActivityFilter;  


                //get all the photos forr the site  and sub items
                int p1 = ncint(f.properties.PHOTO_ID_1);
                int p2 = ncint(f.properties.PHOTO_ID_2);
                int p3 = ncint(f.properties.PHOTO_ID_3);

                List<int> photoList = new List<int>(); 
                photoListAppend(ref photoList, p1);
                photoListAppend(ref photoList, p2);
                photoListAppend(ref photoList, p3);

                foreach (int pid in tempPhotos)
                {
                    photoListAppend(ref photoList, pid);
                }

                f.photos = photoList;

                f.properties.originalComments = f.properties.COMMENTS;
                if (f.properties.COMMENTS == null || f.properties.COMMENTS.ToString().Trim() == "")
                {
                    foreach (Site.Activity a in f.activities)
                    {
                        if (a.comment != null && a.comment.ToString().Trim() != "")
                        {
                            f.properties.COMMENTS = a.comment.ToString();
                            break;
                        }
                    }
                }
            }





            //RELICS ################################################
            //RELICS ################################################
            //RELICS ################################################
            //RELICS ################################################
            //RELICS ################################################
            //RELICS ################################################
            foreach (Relic.Feature f in data.relic.features)
            {
                
                f.relicFeatures = new List<string>();
                f.filters = new Relic.Filters();
                f.filters.access = new List<string>();
                f.activities = new List<Relic.Activity>();


                int p1 = ncint(f.properties.PHOTO_ID_1);
                int p2 = ncint(f.properties.PHOTO_ID_2);
                int p3 = ncint(f.properties.PHOTO_ID_3);

                List<int> photoList = new List<int>();
                photoListAppend(ref photoList, p1);
                photoListAppend(ref photoList, p2);
                photoListAppend(ref photoList, p3);
                f.photos = photoList; 

                if (ncustr(f.properties.DIS_ACCESS) != "")
                {
                    Relic.Activity a = new Relic.Activity();
                    if (f.properties.DIS_ACCESS.ToUpper().Contains("PARTIAL"))
                    {
                        f.relicFeatures.Add("disabledAccessPartial");
                        a.activityType = "disabledAccessPartial";
                        a.comment = "Generally accessible subject to site constraints.";
                        f.activities.Add(a);
                    }
                    else
                    if (f.properties.DIS_ACCESS.ToUpper().Contains("FULL"))
                    {

                        f.relicFeatures.Add("disabledAccessFull");
                        a.activityType = "disabledAccessFull";
                        a.comment = "Compliant with Australian Standards.";
                        f.activities.Add(a);
                    }
                    else
                    {
                        f.relicFeatures.Add("disabledAccess");
                        a.activityType = "disabledAccess";
                    }
                }



            }











            // TRACKS ######################################################################
            // TRACKS ######################################################################
            // TRACKS ######################################################################
            // TRACKS ######################################################################
            // TRACKS ######################################################################
            bool walking, mountainBiking, horseRiding, fourWheelDriving, trailBikeRiding, scenicDriving, treeViewing;
            bool walkingGrade1, walkingGrade2, walkingGrade3, walkingGrade4, walkingGrade5;



            foreach (Track.Feature f in data.track.features)
            {
                f.trackFeatures = new List<string>();

                foreach (Hut.Feature feh in data.hut.features)
                {
                    if (f.properties.SERIAL_NO == feh.properties.IS_PART_OF_TRACK)
                    {
                        f.trackFeatures.Add("hut");
                    }
                }
                foreach (Carpark.Feature fec in data.carpark.features)
                {
                    if (f.properties.SERIAL_NO == fec.properties.IS_PART_OF_TRACK)
                    {
                        f.trackFeatures.Add("carpark");
                    }
                }
                foreach (Asset.Feature fec in data.asset.features)
                {
                    if (f.properties.SERIAL_NO == fec.properties.IS_PART_OF)
                    {
                        foreach(string af in fec.assetFeatures)
                        {
                            f.trackFeatures.Add(af);
                        } 
                        
                    }
                }
                foreach (Site.Feature fec in data.site.features)
                {
                    if (f.properties.SERIAL_NO == fec.properties.IS_PART_OF)
                    {
                        foreach (string sf in fec.siteFeatures)
                        {
                            f.trackFeatures.Add(sf);
                        }

                    }
                }


                walking = false; mountainBiking = false; horseRiding = false; fourWheelDriving = false; trailBikeRiding = false; scenicDriving = false; treeViewing = false;
                walkingGrade1 = false; walkingGrade2 = false; walkingGrade3 = false; walkingGrade4 = false; walkingGrade5 = false;

                if (ncstr(f.properties.DIS_ACCESS) != "")
                {
                    if(f.properties.DIS_ACCESS.ToUpper().Contains("PARTIAL"))
                    {
                        f.trackFeatures.Add("disabledAccessPartial");
                    }
                    else
                    if (f.properties.DIS_ACCESS.ToUpper().Contains("FULL"))
                    {
                        f.trackFeatures.Add("disabledAccessFull");
                    }
                    else
                    {
                        f.trackFeatures.Add("disabledAccessOther");
                    }
                }
                if (ncustr(f.properties.TREE_VIEW) != "") {f.trackFeatures.Add("treeViewing"); treeViewing = true;}
                if (ncustr(f.properties.WILDERNESS) != "") f.trackFeatures.Add("wilderness");

                if (ncustr(f.properties.F_ACTIVITY) != "") { f.trackFeatures.Add("fourWheelDriving"); fourWheelDriving = true; }
                if (ncstr(f.properties.F_GRADE).ToLower().Contains("easy")) f.trackFeatures.Add("fourWheelDrivingEasy");
                else if (ncstr(f.properties.F_GRADE).ToLower().Contains("med")) f.trackFeatures.Add("fourWheelDrivingMedium");
                else if (ncstr(f.properties.F_GRADE).ToLower().Replace(" ", "").Contains("ver")) f.trackFeatures.Add("fourWheelDrivingVeryDifficult");
                else if (ncstr(f.properties.F_GRADE).ToLower().Replace(" ", "").Contains("dif")) f.trackFeatures.Add("fourWheelDrivingDifficult");

                if (ncustr(f.properties.H_ACTIVITY) != "") { f.trackFeatures.Add("horseRiding"); horseRiding = true; }
                if (ncstr(f.properties.H_GRADE).ToLower().Contains("basic")) f.trackFeatures.Add("horseRidingBasic");
                else if (ncstr(f.properties.H_GRADE).ToLower().Contains("mod")) f.trackFeatures.Add("horseRidingModerate");
                else if (ncstr(f.properties.H_GRADE).ToLower().Contains("inter")) f.trackFeatures.Add("horseRidingIntermediate");
                else if (ncstr(f.properties.H_GRADE).ToLower().Contains("adv")) f.trackFeatures.Add("horseRidingAdvanced");


                if (ncustr(f.properties.M_ACTIVITY) != "") { f.trackFeatures.Add("mountainBiking"); mountainBiking = true; }
                if (ncstr(f.properties.M_DIFFICULT).ToLower().Replace(" ","").Contains("veryeasy")) f.trackFeatures.Add("mtbVeryEasy");
                else if (ncstr(f.properties.M_DIFFICULT).ToLower().Contains("easy")) f.trackFeatures.Add("mtbEasy");
                else if (ncstr(f.properties.M_DIFFICULT).ToLower().Contains("inter")) f.trackFeatures.Add("mtbIntermediate");
                else if (ncstr(f.properties.M_DIFFICULT).ToLower().Replace(" ", "").Contains("verydiff")) f.trackFeatures.Add("mtbExtreme");
                else if (ncstr(f.properties.M_DIFFICULT).ToLower().Contains("dif")) f.trackFeatures.Add("mtbDifficult");
                else if (ncstr(f.properties.M_DIFFICULT).ToLower().Contains("ext")) f.trackFeatures.Add("mtbExtreme");
 
                if(f.properties.SERIAL_NO == "625")
                {
                    int cc = 5;
                }

                if (ncustr(f.properties.W_ACTIVITY) != "") { f.trackFeatures.Add("walking"); walking = true; }
                if (ncustr(f.properties.W_LEVEL) == "GRADE 1") { f.trackFeatures.Add("walkingGrade1"); walkingGrade1 = true; }
                else if (ncustr(f.properties.W_LEVEL) == "GRADE 2") { f.trackFeatures.Add("walkingGrade2"); walkingGrade2 = true; }
                else if (ncustr(f.properties.W_LEVEL) == "GRADE 3") { f.trackFeatures.Add("walkingGrade3"); walkingGrade3 = true; }
                else if (ncustr(f.properties.W_LEVEL) == "GRADE 4") { f.trackFeatures.Add("walkingGrade4"); walkingGrade4 = true; }
                else if (ncustr(f.properties.W_LEVEL) == "GRADE 5") { f.trackFeatures.Add("walkingGrade5"); walkingGrade5 = true; }

                if (ncustr(f.properties.T_ACTIVITY) != "") { f.trackFeatures.Add("trailBikeRiding"); trailBikeRiding = true; }
                if (ncstr(f.properties.T_GRADE).ToLower().Contains("easy")) f.trackFeatures.Add("trailBikeRidingEasy");
                else if (ncstr(f.properties.T_GRADE).ToLower().Contains("med")) f.trackFeatures.Add("trailBikeRidingMedium");
                else if (ncstr(f.properties.T_GRADE).ToLower().Contains("ver")) f.trackFeatures.Add("trailBikeRidingVeryDifficult");
                else if (ncstr(f.properties.T_GRADE).ToLower().Contains("dif")) f.trackFeatures.Add("trailBikeRidingDifficult");

                if (ncustr(f.properties.D_ACTIVITY) != "") { f.trackFeatures.Add("scenicDriving"); scenicDriving = true; }
                if (ncstr(f.properties.D_GRADE).ToLower().Contains("easy")) f.trackFeatures.Add("scenicDrivingEasy");
                else if (ncstr(f.properties.D_GRADE).ToLower().Contains("med")) f.trackFeatures.Add("scenicDrivingMedium");


                f.qualities = new Track.Qualities(); 
                f.qualities.track_class = f.properties.TRK_CLASS;
                f.qualities.markings = f.properties.QUAL_MARK;
                f.qualities.quality = f.properties.QUAL_TRACK; 

                f.activities = new List<Track.Activity>();
                if (walking)
                {
                    Track.Activity act = new Track.Activity();
                    act.activityType = "walking";
                    act.comment = f.properties.W_COMMENT;
                    //act.grade = f.properties.W_GRADE;
                    //SCOTT 20112017
                    act.grade = f.properties.W_LEVEL;

                    decimal dist = 0;
                    decimal.TryParse(ncstr(f.properties.W_DISTANCE), out dist);
                    string duom = ncstr(f.properties.W_DISTUOM);
                    act.distance_km = dist;
                    if (duom.ToLower() == "m") act.distance_km = act.distance_km * 1000;
                    act.distance = dist.ToString("0.0") + duom;
                    act.duration_measure = ncstr(f.properties.W_MEASURE);
                    string dur0 = ncstr(f.properties.W_DURDESC).Replace(" ","").ToUpper();
                    object dur;
                    if (dur0 == "") dur = "";
                    else if (dur0.Contains("<1HOUR")) dur = "Under 1 hour";
                    else if (dur0.Contains("1-4HOUR")) dur = "1 to 4 hours";
                    else if (dur0.Contains(">4HOUR")) dur = "4+ hours";
                    else if (dur0.Contains("MULTIPLEDAYS")) dur = "Multiday";
                    else if (dur0.Contains("OVERNIGHT")) dur = "Overnight";
                    else
                    {
                        dur = dur0;
                    }
                    act.duration = dur;


                    string gc = "";
                    gc = ncappend(gc, ncstr(f.properties.W_GRADIENT));
                    gc = ncappend(gc, ncstr(f.properties.QUAL_TRACK));
                    gc = ncappend(gc, ncstr(f.properties.QUAL_MARK));
                    gc = ncappend(gc, ncstr(f.properties.W_STEPS));
                    gc = ncappend(gc, ncstr(f.properties.W_EXPERT));

                    if (gc == "") act.grade_comment = null;
                    else act.grade_comment = gc;


                    f.activities.Add(act);
                }

                if (fourWheelDriving)
                {
                    Track.Activity act = new Track.Activity();
                    act.activityType = "fourWheelDriving";
                    act.comment = f.properties.F_COMMENT;
                    act.grade = f.properties.F_GRADE;

                    decimal dist = 0;
                    decimal.TryParse(ncstr(f.properties.F_DISTANCE), out dist);
                    string duom = ncstr(f.properties.F_DISTUOM);
                    act.distance_km = dist;
                    if (duom.ToLower() == "m") act.distance_km = act.distance_km * 1000;
                    act.distance = dist.ToString("0.0") + duom;
                    act.duration_measure = ncstr(f.properties.F_MEASURE);
                    string dur0 = ncstr(f.properties.W_DURDESC).Replace(" ", "").ToUpper();
                    object dur;
                    if (dur0 == "") dur = "";
                    else if (dur0.Contains("<1HOUR")) dur = "Under 1 hour";
                    else if (dur0.Contains("1-4HOUR")) dur = "1 to 4 hours";
                    else if (dur0.Contains(">4HOUR")) dur = "4+ hours";
                    else
                    {
                        dur = dur0;
                    }
                    act.duration = dur;

                    f.activities.Add(act);
                }


                if (horseRiding)
                {
                    Track.Activity act = new Track.Activity();
                    act.activityType = "horseRiding";
                    act.comment = f.properties.H_COMMENT;
                    act.grade = f.properties.H_GRADE;

                    decimal dist = 0;
                    decimal.TryParse(ncstr(f.properties.H_DISTANCE), out dist);
                    string duom = ncstr(f.properties.H_DISTUOM);
                    act.distance_km = dist;
                    if (duom.ToLower() == "m") act.distance_km = act.distance_km * 1000;
                    act.distance = dist.ToString("0.0") + duom;
                    act.duration_measure = ncstr(f.properties.H_MEASURE);
                    string dur0 = ncstr(f.properties.W_DURDESC).Replace(" ", "").ToUpper();
                    object dur;
                    if (dur0 == "") dur = "";
                    else if (dur0.Contains("<1HOUR")) dur = "Under 1 hour";
                    else if (dur0.Contains("1-4HOUR")) dur = "1 to 4 hours";
                    else if (dur0.Contains(">4HOUR")) dur = "4+ hours";
                    else
                    {
                        dur = dur0;
                    }
                    act.duration = dur;

                    f.activities.Add(act);
                }

                if (trailBikeRiding)
                {
                    Track.Activity act = new Track.Activity();
                    act.activityType = "trailBikeRiding";
                    act.comment = f.properties.T_COMMENT;
                    act.grade = f.properties.T_GRADE;

                    decimal dist = 0;
                    decimal.TryParse(ncstr(f.properties.T_DISTANCE), out dist);
                    string duom = ncstr(f.properties.T_DISTUOM);
                    act.distance_km = dist;
                    if (duom.ToLower() == "m") act.distance_km = act.distance_km * 1000;
                    act.distance = dist.ToString("0.0") + duom;
                    act.duration_measure = ncstr(f.properties.T_MEASURE);
                    string dur0 = ncstr(f.properties.W_DURDESC).Replace(" ", "").ToUpper();
                    object dur;
                    if (dur0 == "") dur = "";
                    else if (dur0.Contains("<1HOUR")) dur = "Under 1 hour";
                    else if (dur0.Contains("1-4HOUR")) dur = "1 to 4 hours";
                    else if (dur0.Contains(">4HOUR")) dur = "4+ hours";
                    else
                    {
                        dur = dur0;
                    }
                    act.duration = dur;

                    string gc = "";
                    gc = ncappend(gc, ncstr(f.properties.T_ACTIVITY));

                    if (gc == "") act.grade_comment = null;
                    else act.grade_comment = gc;

                    f.activities.Add(act);
                }

                if (treeViewing)
                {
                    Track.Activity act = new Track.Activity();
                    act.activityType = "treeViewing";
                    act.comment = f.properties.TREEVIEW_C;
                    f.activities.Add(act);
                }

                if (mountainBiking)
                {
                    Track.Activity act = new Track.Activity();
                    act.activityType = "mountainBiking";
                    act.comment = f.properties.M_COMMENT;
                    act.grade = f.properties.M_DIFFICULT;


                    decimal dist = 0;
                    decimal.TryParse(ncstr(f.properties.M_DISTANCE), out dist);
                    string duom = ncstr(f.properties.M_DISTUOM);
                    act.distance_km = dist;
                    if (duom.ToLower() == "m") act.distance_km = act.distance_km * 1000;
                    act.distance = dist.ToString("0.0") + duom;
                    act.duration_measure = ncstr(f.properties.M_MEASURE);
                    string dur0 = ncstr(f.properties.W_DURDESC).Replace(" ", "").ToUpper();
                    object dur;
                    if (dur0 == "") dur = "";
                    else if (dur0.Contains("<1HOUR")) dur = "Under 1 hour";
                    else if (dur0.Contains("1-4HOUR")) dur = "1 to 4 hours";
                    else if (dur0.Contains(">4HOUR")) dur = "4+ hours";
                    else
                    {
                        dur = dur0;
                    }
                    act.duration = dur;

                    string gc = "";
                    gc = ncappend(gc, ncstr(f.properties.M_FITNESS));

                    if (gc == "") act.grade_comment = null;
                    else act.grade_comment = gc;

                    f.activities.Add(act);
                }




                if (scenicDriving)
                {
                    Track.Activity act = new Track.Activity();
                    act.activityType = "scenicDriving";
                    act.comment = f.properties.D_COMMENT;
                    act.grade = f.properties.D_GRADE;

                    decimal dist = 0;
                    decimal.TryParse(ncstr(f.properties.D_DISTANCE), out dist);
                    string duom = ncstr(f.properties.D_DISTUOM);
                    act.distance_km = dist;
                    if (duom.ToLower() == "m") act.distance_km = act.distance_km * 1000;
                    act.distance = dist.ToString("0.0") + duom;
                    act.duration_measure = ncstr(f.properties.D_MEASURE);
                    string dur0 = ncstr(f.properties.W_DURDESC).Replace(" ", "").ToUpper();
                    object dur;
                    if (dur0 == "") dur = "";
                    else if (dur0.Contains("<1HOUR")) dur = "Under 1 hour";
                    else if (dur0.Contains("1-4HOUR")) dur = "1 to 4 hours";
                    else if (dur0.Contains(">4HOUR")) dur = "4+ hours";
                    else
                    {
                        dur = dur0;
                    }
                    act.duration = dur;

                    f.activities.Add(act);
                }



                //############################################################
                //now get photos and amenitiesFilter and activitiesFilter

                int pt1 = ncint(f.properties.PHOTO_ID_1);
                int pt2 = ncint(f.properties.PHOTO_ID_2);
                int pt3 = ncint(f.properties.PHOTO_ID_3);

                if(f.properties.SERIAL_NO == "20178")
                {
                    int test = 1;
                }
                List<int> photoList = new List<int>();
                photoListAppend(ref photoList, pt1);
                photoListAppend(ref photoList, pt2);
                photoListAppend(ref photoList, pt3);

                List<string> amenitiesFilter = new List<string>();
                List<string> activitiesFilter = new List<string>();
                List<string> accessFilter = new List<string>();

                foreach (Site.Feature sf in data.site.features)
                {
                    if(f.properties.SERIAL_NO == sf.properties.IS_PART_OF)
                    {
                        int p1 = ncint(sf.properties.PHOTO_ID_1);
                        int p2 = ncint(sf.properties.PHOTO_ID_2);
                        int p3 = ncint(sf.properties.PHOTO_ID_3);
                        photoListAppend(ref photoList, p1);
                        photoListAppend(ref photoList, p2);
                        photoListAppend(ref photoList, p3);

                        foreach (string ss in sf.siteFeatures)
                        {
                            if (ss == "carpark") amenitiesFilter.Add(ss); 
                            if (ss == "hut") amenitiesFilter.Add(ss);
                            if (ss == "disabledAccess") accessFilter.Add(ss);
                            if (ss == "caravaning") accessFilter.Add(ss);
                        }
                        foreach (Site.Activity act in sf.activities)
                        {
                            string ss = act.activityType;
                            activitiesFilter.Add(ss);

                            Track.Activity act2 = new Track.Activity();
                            act2.activityType = act.activityType;
                            act2.comment = act.comment;
                            f.activities.Add(act2);  
                        }
                    }
                }
                foreach (Relic.Feature sf in data.relic.features)
                {
                    if (f.properties.SERIAL_NO == sf.properties.IS_PART_OF)
                    {
                        int p1 = ncint(sf.properties.PHOTO_ID_1);
                        int p2 = ncint(sf.properties.PHOTO_ID_2);
                        int p3 = ncint(sf.properties.PHOTO_ID_3);
                        photoListAppend(ref photoList, p1);
                        photoListAppend(ref photoList, p2);
                        photoListAppend(ref photoList, p3);
                    }
                }
                foreach (Carpark.Feature sf in data.carpark.features)
                {
                    if (f.properties.SERIAL_NO == sf.properties.IS_PART_OF_TRACK)
                    {
                        int p1 = ncint(sf.properties.PHOTO_ID_1);
                        int p2 = ncint(sf.properties.PHOTO_ID_2);
                        int p3 = ncint(sf.properties.PHOTO_ID_3);
                        photoListAppend(ref photoList, p1);
                        photoListAppend(ref photoList, p2);
                        photoListAppend(ref photoList, p3);
                        amenitiesFilter.Add("carpark");
                    }
                }
                foreach (Hut.Feature sf in data.hut.features)
                {
                    if (f.properties.SERIAL_NO == sf.properties.IS_PART_OF_TRACK)
                    {
                        int p1 = ncint(sf.properties.PHOTO_ID_1);
                        int p2 = ncint(sf.properties.PHOTO_ID_2);
                        int p3 = ncint(sf.properties.PHOTO_ID_3);
                        photoListAppend(ref photoList, p1);
                        photoListAppend(ref photoList, p2);
                        photoListAppend(ref photoList, p3);
                        amenitiesFilter.Add("hut");
                    }
                }
                foreach(Asset.Feature sf in data.asset.features)
                {
                    if (f.properties.SERIAL_NO == sf.properties.IS_PART_OF || f.properties.SERIAL_NO == sf.properties.IS_PART_OF_TRACK)
                    {
                        int p1 = ncint(sf.properties.PHOTO_ID_1);
                        int p2 = ncint(sf.properties.PHOTO_ID_2);
                        int p3 = ncint(sf.properties.PHOTO_ID_3);
                        photoListAppend(ref photoList, p1);
                        photoListAppend(ref photoList, p2);
                        photoListAppend(ref photoList, p3);
                        foreach (string ss in sf.assetFeatures)
                        {
                            if (ss == "informationShelter") amenitiesFilter.Add(ss);
                            if (ss == "toiletBlock")
                            {
                                amenitiesFilter.Add(ss);
                            }
                            if (ss == "picnicShelter") amenitiesFilter.Add(ss);
                            if (ss == "viewingPlatform") amenitiesFilter.Add(ss);
                        }

                    }


                }

                f.photos = photoList;



                // ####################################################
                // set filter strings
                // ####################################################
                f.filters = new Track.Filters();
                f.filters.access = new List<string>();
                f.filters.activities = new List<string>();
                f.filters.amenities = new List<string>();
                f.filters.difficulty = new List<string>();
                f.filters.duration = new List<string>();
                f.filters.trackType = new List<string>();

                //trackType
                if (walking) f.filters.trackType.Add("walking");
                if (mountainBiking) f.filters.trackType.Add("mountainBiking");
                if (trailBikeRiding) f.filters.trackType.Add("trailBikeRiding");
                if (fourWheelDriving) f.filters.trackType.Add("fourWheelDriving");
                if (horseRiding) f.filters.trackType.Add("horseRiding");
                if (scenicDriving) f.filters.trackType.Add("scenicDriving"); 




                if(walking)
                {
                    if (walkingGrade1) f.filters.difficulty.Add("walkingGrade1");
                    if (walkingGrade2) f.filters.difficulty.Add("walkingGrade2");
                    if (walkingGrade3) f.filters.difficulty.Add("walkingGrade3");
                    if (walkingGrade4) f.filters.difficulty.Add("walkingGrade4");
                    if (walkingGrade5) f.filters.difficulty.Add("walkingGrade5");
                }
                if(mountainBiking)
                {
                    string ss = ncstr(f.properties.M_DIFFICULT).Replace(" ","").ToLower();
                    if (ss.Contains("inter")) f.filters.difficulty.Add("mtbIntermediate");
                    else if (ss.Contains("verydiff")) f.filters.difficulty.Add("mtbExtreme");
                    else if (ss.Contains("difficult")) f.filters.difficulty.Add("mtbDifficult");
                    else if (ss.Contains("ext")) f.filters.difficulty.Add("mtbExtreme");
                    else if (ss.Contains("veryeasy")) f.filters.difficulty.Add("mtbVeryEasy");
                    else if (ss.Contains("easy")) f.filters.difficulty.Add("mtbEasy");
                }

                if (horseRiding)
                {
                    string ss = ncstr(f.properties.H_GRADE).ToLower();
                    if (ss.Contains("basic")) f.filters.difficulty.Add("horseRidingBasic");
                    if (ss.Contains("mod")) f.filters.difficulty.Add("horseRidingModerate");
                    if (ss.Contains("inter")) f.filters.difficulty.Add("horseRidingIntermediate");
                    if (ss.Contains("adv")) f.filters.difficulty.Add("horseRidingAdvanced");


                }

                if (fourWheelDriving)
                {
                    string ss = ncstr(f.properties.F_GRADE).ToLower().Replace(" ", "");
                    if (ss.Contains("easy")) f.filters.difficulty.Add("fourWheelDrivingEasy");
                    else if (ss.Contains("med")) f.filters.difficulty.Add("fourWheelDrivingMedium");
                    else if (ss.Contains("ver")) f.filters.difficulty.Add("fourWheelDrivingVeryDifficult");
                    else if (ss.Contains("dif")) f.filters.difficulty.Add("fourWheelDrivingDifficult");
                }


                if(trailBikeRiding)
                {
                    string ss = ncstr(f.properties.T_GRADE).ToLower();
                    if (ss.Contains("easy")) f.filters.difficulty.Add("trailBikeRidingEasy");
                    else if (ss.Contains("med")) f.filters.difficulty.Add("trailBikeRidingMedium");
                    else if (ss.Contains("ver")) f.filters.difficulty.Add("trailBikeRidingVeryDifficult");
                    else if (ss.Contains("dif")) f.filters.difficulty.Add("trailBikeRidingDifficult");

                }

                if (scenicDriving)
                {
                    string ss = ncstr(f.properties.D_GRADE).ToLower();
                    if (ss.Contains("easy")) f.filters.difficulty.Add("scenicDrivingEasy");
                    else if (ss.Contains("med")) f.filters.difficulty.Add("scenicDrivingMedium");
                }



                foreach (Track.Activity a in f.activities)
                {
                    string dd = ncstr(a.duration).ToLower().Replace(" ", "");  
                    if(dd != "")
                    {
                        if (dd.Contains("under1hour")) f.filters.duration.Add("short");
                        else if (dd.Contains("1to4")) f.filters.duration.Add("medium");
                        else if (dd.Contains("4+")) f.filters.duration.Add("day");
                        else if (dd.Contains("overnight")) f.filters.duration.Add("overnight");
                        else if (dd.Contains("short")) f.filters.duration.Add("short");
                        else if (dd.Contains("medium")) f.filters.duration.Add("medium");
                        else if (dd.Contains("multiday")) f.filters.duration.Add("multiday");
                        else
                        {
                            f.filters.duration.Add(dd);
                        }
                    }
                }


                f.filters.amenities = amenitiesFilter;

                if (ncstr(f.properties.TREE_VIEW) != "") activitiesFilter.Add("treeViewing");
                if (ncstr(f.properties.WILDERNESS) != "") activitiesFilter.Add("wilderness");
                f.filters.activities = activitiesFilter;

                if (ncstr(f.properties.DIS_ACCESS) != "") accessFilter.Add("disabledAccess");
                f.filters.access = accessFilter;



                f.properties.originalComments = f.properties.COMMENTS;
                if (String.IsNullOrEmpty(f.properties.COMMENTS))
                {
                    foreach(Track.Activity  a in f.activities)
                    {
                        if(a.comment != null && a.comment.ToString().Trim() != "")
                        {
                            f.properties.COMMENTS = a.comment.ToString();
                            break;
                        }
                    }
                }

            }

        }


        public class Track
        {
            public class Geometry
            {
                public string type { get; set; }
                public List<List<object>> coordinates { get; set; }
            }

            public class Properties
            {
                public object SE_ANNO_CAD_DATA { get; set; }
                public string SERIAL_NO { get; set; }
                public string NAME { get; set; }
                public string ASSET_CLS { get; set; }
                public string ACCESS_DSC { get; set; }
                public string COMMENTS { get; set; }
                public string originalComments { get; set; }
                public string DIS_ACCESS { get; set; }
                public string EXPORT_DATE { get; set; }
                public string PUBLISHED { get; set; }
                public string TRK_CLASS { get; set; }
                public object WILDERNESS { get; set; }
                public string CLOS_STAT { get; set; }
                public string CLOS_DESC { get; set; }
                public string CLOS_START { get; set; }
                public string CLOS_OPEN { get; set; }
                public string CLOS_REAS { get; set; }
                public string F_ACTIVITY { get; set; }
                public string F_COMMENT { get; set; }
                public string F_DURDESC { get; set; }
                public string F_GRADE { get; set; }
                public string F_DISTANCE { get; set; }
                public string F_DISTUOM { get; set; }
                public object F_TIME { get; set; }
                public object F_TIMEUOM { get; set; }
                public string F_MEASURE { get; set; }
                public object H_ACTIVITY { get; set; }
                public object H_COMMENT { get; set; }
                public object H_DURDESC { get; set; }
                public object H_GRADE { get; set; }
                public object H_DISTANCE { get; set; }
                public object H_DISTUOM { get; set; }
                public object H_TIME { get; set; }
                public object H_TIMEUOM { get; set; }
                public object H_MEASURE { get; set; }
                public string QUAL_MARK { get; set; }
                public string QUAL_TRACK { get; set; }
                public string W_ACTIVITY { get; set; }
                public string W_COMMENT { get; set; }
                public string W_DISTANCE { get; set; }
                public string W_DISTUOM { get; set; }
                public string W_DURDESC { get; set; }
                public string W_EXPERT { get; set; }
                public string W_GRADE { get; set; }
                public string W_GRADIENT { get; set; }
                public string W_LEVEL { get; set; }
                public string W_MEASURE { get; set; }
                public string W_STEPS { get; set; }
                public string W_TIME { get; set; }
                public string W_TIMEUOM { get; set; }
                public object M_ACTIVITY { get; set; }
                public object M_COMMENT { get; set; }
                public object M_DURDESC { get; set; }
                public object M_DISTANCE { get; set; }
                public object M_DISTUOM { get; set; }
                public object M_TIME { get; set; }
                public object M_TIMEUOM { get; set; }
                public object M_MEASURE { get; set; }
                public object M_FITNESS { get; set; }
                public object M_DIFFICULT { get; set; }
                public string D_ACTIVITY { get; set; }
                public string D_GRADE { get; set; }
                public string D_DISTANCE { get; set; }
                public string D_DISTUOM { get; set; }
                public string D_TIME { get; set; }
                public string D_TIMEUOM { get; set; }
                public string D_MEASURE { get; set; }
                public string D_COMMENT { get; set; }
                public string D_DURDESC { get; set; }
                public object T_ACTIVITY { get; set; }
                public object T_GRADE { get; set; }
                public object T_DISTANCE { get; set; }
                public object T_DISTUOM { get; set; }
                public object T_TIME { get; set; }
                public object T_TIMEUOM { get; set; }
                public object T_MEASURE { get; set; }
                public object T_COMMENT { get; set; }
                public object T_DURDESC { get; set; }
                public object PADDLING { get; set; }
                public object PADDLING_C { get; set; }
                public object TREE_VIEW { get; set; }
                public object TREEVIEW_C { get; set; }
                public string PHOTO_ID_1 { get; set; }
                public string PHOTO_ID_2 { get; set; }
                public string PHOTO_ID_3 { get; set; }
                public int LAYER_EDIT_DATE { get; set; }
            }

            public class Feature
            {
                public string type { get; set; }
                public string id { get; set; }
                public Geometry geometry { get; set; }
                public string geometry_name { get; set; }
                public Properties properties { get; set; }
                public List<string> trackFeatures { get; set; }
                public List<Activity> activities { get; set; }
                public Qualities qualities { get; set; }
                public List<int> photos { get; set; }
                public Filters filters { get; set; }
            }


            public class Filters
            {
                public List<string> trackType { get; set; }
                public List<string> difficulty { get; set; }
                public List<string> duration { get; set; }
                public List<string> amenities { get; set; }
                public List<string> activities { get; set; }
                public List<string> access { get; set; }
            }


            public class Qualities
            {
                public object track_class { get; set; }
                public object quality { get; set; }
                public object markings { get; set; }
            }

            public class Activity
            {
                public string activityType { get; set; }
                public object comment { get; set; }
                public object grade_comment { get; set; }
                public object grade { get; set; }
                public object distance { get; set; }
                public decimal distance_km { get; set; }
                public object duration { get; set; }
                public object duration_level { get; set; } //short, medium, day, overnight
                public int duration_max_hrs { get; set; }
                public object duration_measure { get; set; }
            }

            public class Properties2
            {
                public string name { get; set; }
            }

            public class Crs
            {
                public string type { get; set; }
                public Properties2 properties { get; set; }
            }

            public class RootObject
            {
                public string type { get; set; }
                public int totalFeatures { get; set; }
                public List<Feature> features { get; set; }
                public Crs crs { get; set; }
            }

        }



        public class Track2
        {

            public class Geometry
            {
                public string type { get; set; }
                public List<List<List<double>>> coordinates { get; set; }
            }

            public class Properties
            {
                public int OBJECTID { get; set; }
                public object ENTITY_ID { get; set; }
                public string SERIAL_NO { get; set; }
                public string NAME { get; set; }
                public string ASSET_CLS { get; set; }
                public string ACCESS_DSC { get; set; }
                public string COMMENTS { get; set; }
                public object originalComments { get; set; }
                public string DIS_ACCESS { get; set; }
                public string EXPORT_DATE { get; set; }
                public string PUBLISHED { get; set; }
                public string TRK_CLASS { get; set; }
                public object V_NUMBER { get; set; }
                public object WILDERNESS { get; set; }
                public string F_ACTIVITY { get; set; }
                public object F_COMMENT { get; set; }
                public string F_DURDESC { get; set; }
                public string F_GRADE { get; set; }
                public string F_DISTANCE { get; set; }
                public string F_DISTUOM { get; set; }
                public object F_TIME { get; set; }
                public object F_TIMEUOM { get; set; }
                public string F_MEASURE { get; set; }
                public object H_ACTIVITY { get; set; }
                public object H_COMMENT { get; set; }
                public object H_DURDESC { get; set; }
                public object H_GRADE { get; set; }
                public object H_DISTANCE { get; set; }
                public object H_DISTUOM { get; set; }
                public object H_TIME { get; set; }
                public object H_TIMEUOM { get; set; }
                public object H_MEASURE { get; set; }
                public object QUAL_MARK { get; set; }
                public object QUAL_TRACK { get; set; }
                public object W_ACTIVITY { get; set; }
                public object W_COMMENT { get; set; }
                public object W_DISTANCE { get; set; }
                public object W_DISTUOM { get; set; }
                public object W_DURDESC { get; set; }
                public object W_EXPERT { get; set; }
                public object W_GRADE { get; set; }
                public object W_GRADIENT { get; set; }
                public object W_LEVEL { get; set; }
                public object W_MEASURE { get; set; }
                public object W_STEPS { get; set; }
                public object W_TIME { get; set; }
                public object W_TIMEUOM { get; set; }
                public string M_ACTIVITY { get; set; }
                public string M_COMMENT { get; set; }
                public string M_DURDESC { get; set; }
                public string M_DISTANCE { get; set; }
                public string M_DISTUOM { get; set; }
                public object M_TIME { get; set; }
                public object M_TIMEUOM { get; set; }
                public string M_MEASURE { get; set; }
                public object M_FITNESS { get; set; }
                public string M_DIFFICULT { get; set; }
                public object D_ACTIVITY { get; set; }
                public object D_GRADE { get; set; }
                public object D_DISTANCE { get; set; }
                public object D_DISTUOM { get; set; }
                public object D_TIME { get; set; }
                public object D_TIMEUOM { get; set; }
                public object D_MEASURE { get; set; }
                public object D_COMMENT { get; set; }
                public object D_DURDESC { get; set; }
                public object T_ACTIVITY { get; set; }
                public object T_GRADE { get; set; }
                public object T_DISTANCE { get; set; }
                public object T_DISTUOM { get; set; }
                public object T_TIME { get; set; }
                public object T_TIMEUOM { get; set; }
                public object T_MEASURE { get; set; }
                public object T_COMMENT { get; set; }
                public object T_DURDESC { get; set; }
                public string TREE_VIEW { get; set; }
                public string TREEVIEW_C { get; set; }
                public object CLOS_STAT { get; set; }
                public object CLOS_DESC { get; set; }
                public string CLOS_START { get; set; }
                public string CLOS_OPEN { get; set; }
                public object CLOS_REAS { get; set; }
                public int LAYER_EDIT_DATE { get; set; }
                public object PHOTO_ID_1 { get; set; }
                public object PHOTO_ID_2 { get; set; }
                public object PHOTO_ID_3 { get; set; }
            }

            public class Feature
            {
                public string type { get; set; }
                public string id { get; set; }
                public Geometry geometry { get; set; }
                public string geometry_name { get; set; }
                public Properties properties { get; set; }
                public List<string> trackFeatures { get; set; } 
                public List<Activity> activities { get; set; }
                public Qualities qualities { get; set; }
                public List<int> photos { get; set; }
                public Filters filters { get; set; }
            }


            public class Filters
            {
                public List<string> trackType { get; set; }
                public List<string> difficulty { get; set; }
                public List<string> duration { get; set; }
                public List<string> amenities { get; set; }
                public List<string> activities { get; set; }
                public List<string> access { get; set; }
            }


            public class Qualities
            {
                public object track_class { get; set;}
                public object quality { get; set; }
                public object markings { get; set; }
            }

            public class Activity
            {
                public string  activityType { get; set; }
                public object  comment { get; set; }
                public object  grade_comment {  get; set;}
                public object  grade { get; set; }
                public object  distance { get; set; }
                public decimal distance_km { get; set; }
                public object  duration { get; set; }
                public object  duration_level { get; set; } //short, medium, day, overnight
                public int     duration_max_hrs { get; set; }
                public object  duration_measure { get; set; }
            }

            public class Properties2
            {
                public string name { get; set; }
            }

            public class Crs
            {
                public string type { get; set; }
                public Properties2 properties { get; set; }
            }

            public class RootObject
            {
                public string type { get; set; }
                public int totalFeatures { get; set; }
                public List<Feature> features { get; set; }
                public Crs crs { get; set; }
            }

        }

        public class Site
        {
            public class Geometry
            {
                public string type { get; set; }
                public List<double> coordinates { get; set; }
            }

            public class Properties
            {
                public string FAC_TYPE { get; set; }
                public string VERS_DATE { get; set; }
                public string SERIAL_NO { get; set; }
                public string NAME { get; set; }
                public double LATITUDE { get; set; }
                public double LONGITUDE { get; set; }
                public string SITE_CLASS { get; set; }
                public object WILDERNESS { get; set; }
                public string DIS_ACCESS { get; set; }
                public string ACCESS_DSC { get; set; }
                public string FEE { get; set; }
                public object COMMENTS { get; set; }
                public object originalComments { get; set; }
                public object CAMPING { get; set; }
                public object CAMPING_C { get; set; }
                public object CAMPERVANNING { get; set; }
                public object CAMPERVAN_C { get; set; }
                public object CAMPERVAN_TYPE { get; set; }
                public object TB_VISITOR { get; set; }
                public object TBVA_C { get; set; }
                public object HERITAGE { get; set; }
                public object HERITAGE_C { get; set; }
                public object FISHING { get; set; }
                public object FISHING_C { get; set; }
                public object FOSSICKING { get; set; }
                public object FOSSICK_C { get; set; }
                public object HANG_GLIDE { get; set; }
                public object HANG_GLD_C { get; set; }
                public object PADDLING { get; set; }
                public object PADDLING_C { get; set; }
                public object PICNICING { get; set; }
                public object PICNICING_C { get; set; }
                public object PCT_AFRAME { get; set; }
                public object PCT_PEDEST { get; set; }
                public object PCT_OTHER { get; set; }
                public object BBQ_ELEC { get; set; }
                public object BBQ_PIT { get; set; }
                public object BBQ_GAS { get; set; }
                public object BBQ_WOOD { get; set; }
                public object REDUCE_NA { get; set; }
                public object REDUCE_C { get; set; }
                public object ROCKCLIMB { get; set; }
                public object ROCKCLIMB_C { get; set; }
                public object WALKINGDOG { get; set; }
                public object WALKDOG_C { get; set; }
                public object WILDLIFE { get; set; }
                public object WILDLIFE_C { get; set; }
                public string IS_PART_OF { get; set; }
                public string PHOTO_ID_1 { get; set; }
                public string PHOTO_ID_2 { get; set; }
                public string PHOTO_ID_3 { get; set; }
                public string LABEL { get; set; }
                public string ACT_CODE { get; set; }
                public string FAC_CODE { get; set; }
                public string PUBLISHED { get; set; }
                public string TREE_VIEW { get; set; }
                public string TREEVIEW_C { get; set; }
                public string CLOS_STAT { get; set; }
                public string CLOS_DESC { get; set; }
                public string CLOS_START { get; set; }
                public string CLOS_OPEN { get; set; }
                public string CLOS_REAS { get; set; }
                public string SDE_DATE { get; set; }
                public double Y_COORD { get; set; }
                public double X_COORD { get; set; }
                public object SE_ANNO_CAD_DATA { get; set; }
            }


            public class Activity
            {
                public string activityType { get; set; }
                public object comment { get; set; }
            }

            public class Feature
            {
                public string type { get; set; }
                public string id { get; set; }
                public Geometry geometry { get; set; }
                public string geometry_name { get; set; }
                public Properties properties { get; set; }
                public List<string> siteFeatures { get; set; }
                public Filters filters { get; set; }
                public List<Activity> activities { get; set; }
                public List<int> photos { get; set; }
                public object siteDescriptrionFromFirstActivity { get; set; }
            }

            public class Filters
            {
                public List<string> amenities { get; set; }
                public List<string> activities { get; set; }
                public List<string> access { get; set; }
            }

            public class Properties2
            {
                public string name { get; set; }
            }

            public class Crs
            {
                public string type { get; set; }
                public Properties2 properties { get; set; }
            }

            public class RootObject
            {
                public string type { get; set; }
                public int totalFeatures { get; set; }
                public List<Feature> features { get; set; }
                public Crs crs { get; set; }
            }
        }

        public class Hut
        {
            public class Geometry
            {
                public string type { get; set; }
                public List<double> coordinates { get; set; }
            }

            public class Properties
            {
                public string FAC_TYPE { get; set; }
                public string VERS_DATE { get; set; }
                public string SERIAL_NO { get; set; }
                public string NAME { get; set; }
                public string LATITUDE { get; set; }
                public string LONGITUDE { get; set; }
                public object CATEGORY { get; set; }
                public object TYPE { get; set; }
                public string ELEV_STRU { get; set; }
                public object DIS_ACCESS { get; set; }
                public string COMMENTS { get; set; }
                public string HPL_NO { get; set; }
                public string SIGNIF { get; set; }
                public object HISTORIC { get; set; }

                public string IS_PART_OF { get; set; }
                public string IS_PART_OF_TRACK { get; set; }

                public string PHOTO_ID_1 { get; set; }
                public string PHOTO_ID_2 { get; set; }
                public object PHOTO_ID_3 { get; set; }
                public string LABEL { get; set; }
                public string PUBLISHED { get; set; }
                public string SDE_DATE { get; set; }
                public double Y_COORD { get; set; }
                public double X_COORD { get; set; }
                public object SE_ANNO_CAD_DATA { get; set; }
            }

            public class Feature
            {
                public string type { get; set; }
                public string id { get; set; }
                public Geometry geometry { get; set; }
                public string geometry_name { get; set; }
                public Properties properties { get; set; }
            }

            public class Properties2
            {
                public string name { get; set; }
            }

            public class Crs
            {
                public string type { get; set; }
                public Properties2 properties { get; set; }
            }

            public class RootObject
            {
                public string type { get; set; }
                public int totalFeatures { get; set; }
                public List<Feature> features { get; set; }
                public Crs crs { get; set; }
            }
        }

        public class Asset
        {
            public class Geometry
            {
                public string type { get; set; }
                public List<double> coordinates { get; set; }
            }

            public class Properties
            {
                public string FAC_TYPE { get; set; }
                public string VERS_DATE { get; set; }
                public string SERIAL_NO { get; set; }
                public string NAME { get; set; }
                public string LATITUDE { get; set; }
                public string LONGITUDE { get; set; }
                public string ASSET_CLS { get; set; }
                public string CATEGORY { get; set; }
                public string TYPE { get; set; }
                public string ELEV_STRU { get; set; }
                public string DIS_ACCESS { get; set; }
                public string COMMENTS { get; set; }
                public int? QUANTITY { get; set; }
                public int? LENGTH_M { get; set; }
                public int? WIDTH_M { get; set; }
                public string IS_PART_OF { get; set; }
                public string IS_PART_OF_TRACK { get; set; }

                public string PHOTO_ID_1 { get; set; }
                public string PHOTO_ID_2 { get; set; }
                public string PHOTO_ID_3 { get; set; }
                public string LABEL { get; set; }
                public string PUBLISHED { get; set; }
                public string SDE_DATE { get; set; }
                public double X_COORD { get; set; }
                public double Y_COORD { get; set; }
                public object SE_ANNO_CAD_DATA { get; set; }
            }

            public class Feature
            {
                public string type { get; set; }
                public string id { get; set; }
                public Geometry geometry { get; set; }
                public string geometry_name { get; set; }
                public Properties properties { get; set; }
                public List<string> assetFeatures { get; set; }
                public Filters filters { get; set; }
            }

            public class Filters
            {
                public List<string> amenities { get; set; }
            }


            public class Properties2
            {
                public string name { get; set; }
            }

            public class Crs
            {
                public string type { get; set; }
                public Properties2 properties { get; set; }
            }

            public class RootObject
            {
                public string type { get; set; }
                public int totalFeatures { get; set; }
                public List<Feature> features { get; set; }
                public Crs crs { get; set; }
            }
        }

        public class Relic
        {
            public class Geometry
            {
                public string type { get; set; }
                public List<double> coordinates { get; set; }
            }

            public class Properties
            {
                public string FAC_TYPE { get; set; }
                public string VERS_DATE { get; set; }
                public string SERIAL_NO { get; set; }
                public string NAME { get; set; }
                public double LATITUDE { get; set; }
                public double LONGITUDE { get; set; }
                public object CATEGORY { get; set; }
                public object TYPE { get; set; }
                public string ELEV_STRU { get; set; }
                public string DIS_ACCESS { get; set; }
                public string COMMENTS { get; set; }
                public string HPL_NO { get; set; }
                public string SIGNIF { get; set; }
                public string IS_PART_OF { get; set; }
                public string PHOTO_ID_1 { get; set; }
                public string PHOTO_ID_2 { get; set; }
                public string PHOTO_ID_3 { get; set; }
                public string LABEL { get; set; }
                public string PUBLISHED { get; set; }
                public string SDE_DATE { get; set; }
                public double X_COORD { get; set; }
                public double Y_COORD { get; set; }
                public object SE_ANNO_CAD_DATA { get; set; }
            }

            public class Filters
            {
                public List<string> access { get; set; }
            }

            public class Activity
            {
                public string activityType { get; set; }
                public object comment { get; set; }
            }

            public class Feature
            {
                public string type { get; set; }
                public string id { get; set; }
                public Geometry geometry { get; set; }
                public string geometry_name { get; set; }
                public Properties properties { get; set; }
                public List<int> photos { get; set; }
                public List<string> relicFeatures { get; set; }
                public List<Activity> activities { get; set; }
                public Filters filters { get; set; }
            }

            public class Properties2
            {
                public string name { get; set; }
            }

            public class Crs
            {
                public string type { get; set; }
                public Properties2 properties { get; set; }
            }

            public class RootObject
            {
                public string type { get; set; }
                public int totalFeatures { get; set; }
                public List<Feature> features { get; set; }
                public Crs crs { get; set; }
            }
        }

        public class Carpark
        {
            public class Geometry
            {
                public string type { get; set; }
                public List<double> coordinates { get; set; }
            }

            public class Properties
            {
                public string FAC_TYPE { get; set; }
                public string VERS_DATE { get; set; }
                public string SERIAL_NO { get; set; }
                public string NAME { get; set; }
                public string LATITUDE { get; set; }
                public string LONGITUDE { get; set; }
                public object CATEGORY { get; set; }
                public string TYPE { get; set; }
                public object DIS_ACCESS { get; set; }
                public string COMMENTS { get; set; }
                public int? VEHICLES { get; set; }
                public int? TRAILERS { get; set; }

                public string IS_PART_OF { get; set; }
                public string IS_PART_OF_TRACK { get; set; }

                public string PHOTO_ID_1 { get; set; }
                public string PHOTO_ID_2 { get; set; }
                public object PHOTO_ID_3 { get; set; }
                public string LABEL { get; set; }
                public string PUBLISHED { get; set; }
                public string SDE_DATE { get; set; }
                public double X_COORD { get; set; }
                public double Y_COORD { get; set; }
                public object SE_ANNO_CAD_DATA { get; set; }
            }

            public class Feature
            {
                public string type { get; set; }
                public string id { get; set; }
                public Geometry geometry { get; set; }
                public string geometry_name { get; set; }
                public Properties properties { get; set; }
            }

            public class Properties2
            {
                public string name { get; set; }
            }

            public class Crs
            {
                public string type { get; set; }
                public Properties2 properties { get; set; }
            }

            public class RootObject
            {
                public string type { get; set; }
                public int totalFeatures { get; set; }
                public List<Feature> features { get; set; }
                public Crs crs { get; set; }
            }
        }

        public int ncint(object v)
        {
            if (v == null) return 0;
            int rv = 0;
            int.TryParse(v.ToString(), out rv);
            return rv;
        }

        public string ncustr(object v)
        {
            string rv = "";
            if (v == null) v = "";
            rv = v.ToString();

            rv = rv.ToUpper().Trim();
            if (rv == "NONE") rv = "";
            return rv;
        }
        public string ncstr(object v)
        {
            string rv = "";
            if (v == null) v = "";
            rv = v.ToString().Trim();

            return rv;
        }

        public string ncappend(string s1, string s2)
        {
            if (s1 == null) s1 = "";
            if (s2 == null) s2 = "";

            if (s2.Length <= 0) return s1;
            if (s1.Length <= 0) return s2;
            return s1 + ", " + s2;
        }

        public void photoListAppend (ref List<int> list, int id)
        {
            if (id <= 0) return;
            if(list.Contains(id)) return;

            string fo = _appsettings.ImageFolder;
            string fi = fo + "\\" + id + ".jpg";

            if (System.IO.File.Exists(fi))
            {
                list.Add(id);
            }
        }
    }

}
