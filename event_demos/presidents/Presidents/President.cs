using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Presidents
{
    public class President
    {
        public string FirstName{get; set;}
        public string LastName{get; set;}
        public string NickName { get; set; }
// "1st president"
        public string PresidentNumber { get; set; }
        public string YearsInOffice{get; set;}
        public string Party{get; set;}
        public string State{get; set;}
        public string VP { get; set; }
        public string Summary{get; set;}

        public Uri Image { get; set; }
        public ImageSource ImageSource
        {
            get
            {
                return new BitmapImage(this.Image);
            }
        }

        public string AutomationImageName
        {
            get
            {
                return "Picture of " + FullName;
            }
        }

        public int TermLength
        {
            get
            {
                int dashPos = YearsInOffice.IndexOf('-');
                if (dashPos == -1) return 1;
                string start = YearsInOffice.Substring(0, dashPos);
                string end = YearsInOffice.Substring(dashPos + 1, 4);
                int result = int.Parse(end) - int.Parse(start);
                return result;
            }
        }

        public string YearsInParens
        {
            get
            {
                string result = "";
                int spacePos = YearsInOffice.IndexOf(' ');
                if (spacePos == -1)
                    result = YearsInOffice;
                else
                    result = YearsInOffice.Substring(0, spacePos);
                result = "(" + result + ")";
                return result;
            }
        }


        public string FullName
        {
            get { return FirstName + " " + LastName;  }
        }

        public static President[] AllPresidents = new President[] {
            new President()
            {
                FirstName="George",
                LastName="Washington",
                NickName="Father of our country",
                PresidentNumber="1st president",
                YearsInOffice="1789-1797 (2 terms)",
                Party="Independent",
                State="Virginia",
                VP="John Adams",
                Summary="George Washington was the first President of the United States (1789–97), the Commander-in-Chief of the Continental Army during the American Revolutionary War, and one of the Founding Fathers of the United States. He presided over the convention that drafted the current United States Constitution and during his lifetime was called the \"father of his country\"."
                + "\n\nWidely admired for his strong leadership qualities, Washington was unanimously elected president in the first two national elections. He oversaw the creation of a strong, well-financed national government that maintained neutrality in the French Revolutionary Wars, suppressed the Whiskey Rebellion, and won acceptance among Americans of all types.",
                //Image=new Uri("ms-appx:///Assets/Gilbert_Stuart_Williamstown_Portrait_of_George_Washington.jpg"),
                Image=new Uri("ms-appx:///Assets/WashingtonCropped.jpg"),
            },

                    new President()
            {
                FirstName="John",
                LastName="Adams",
                NickName="His Rotundity",
                PresidentNumber="2nd president",
                YearsInOffice="1797-1801",
                Party="Federalist",
                State="Massachusetts",
                VP="Thomas Jefferson",
                Summary="John Adams, Jr. (October 30 [O.S. October 19] 1735 – July 4, 1826) was an American lawyer, author, statesman, and diplomat. He served as the second President of the United States (1797–1801), the first Vice President (1789–97), and as a Founding Father was a leader of American independence from Great Britain.",
                Image=new Uri("ms-appx:///Assets/Official_Presidential_portrait_of_John_Adams_(by_John_Trumbull,_circa_1792).jpg"),
            },
            new President()
            {
                FirstName="Thomas",
                LastName="Jefferson",
                NickName="The Apostle of Democracy",
                PresidentNumber="3rd president",
                YearsInOffice="1801-1809 (2 terms)",
                Party="Democratic-Republican",
                State="Virginia",
                VP="Aaron Burr and George Clinton",
                Summary="Thomas Jefferson (April 13 [O.S. April 2] 1743 – July 4, 1826) was an American Founding Father who was principal author of the Declaration of Independence (1776). He was elected the second Vice President of the United States (1797–1801), serving under John Adams and in 1800 was elected third President (1801–09).",
                //Image=new Uri("ms-appx:///Assets/Thomas_Jefferson_by_Rembrandt_Peale,_1800.jpg"),
                Image=new Uri("ms-appx:///Assets/Official_Presidential_portrait_of_Thomas_Jefferson_(by_Rembrandt_Peale,_1800).jpg"),
            },
            new President()
            {
                FirstName="James",
                LastName="Madison",
                NickName="Little Jemmy",
                PresidentNumber="4th president",
                YearsInOffice="1809-1817 (2 terms)",
                Party="Democratic-Republican",
                State="Virginia",
                VP="George Clinton and Elbridge Gerry",
                Summary="James Madison, Jr. (March 16, 1751 – June 28, 1836) was a political theorist, American statesman, and served as the fourth President of the United States (1809–17). He is hailed as the \"Father of the Constitution\" for his pivotal role in drafting and promoting the U.S. Constitution and the Bill of Rights.",
                //Image=new Uri("ms-appx:///Assets/165px-James_Madison.jpg"),
                Image=new Uri("ms-appx:///Assets/1024px-James_Madison.jpg"),
            },
            new President()
            {
                FirstName="James",
                LastName="Monroe",
                NickName="The Last Cocked Hat",
                PresidentNumber="5th president",
                YearsInOffice="1817-1825 (2 terms)",
                Party="Democratic-Republican",
                State="Virginia",
                VP="Daniel D. Tompkins",
                Summary="James Monroe (April 28, 1758 – July 4, 1831) was the fifth President of the United States, serving between 1817 and 1825. Monroe was the last president who was a Founding Father of the United States and the last president from the Virginian dynasty and the Republican Generation.",
                //Image=new Uri("ms-appx:///Assets/James_Monroe_White_House_portrait_1819.gif"),
                Image=new Uri("ms-appx:///Assets/James_Monroe_White_House_portrait_1819.gif"),
            },
            new President()
            {
                FirstName="John Quincy",
                LastName="Adams",
                NickName="Old Man Eloquent",
                PresidentNumber="6th president",
                YearsInOffice="1825-1829",
                Party="Democratic-Republican",
                State="Massachusetts",
                VP="John C. Calhoun",
                Summary="John Quincy Adams (July 11, 1767 – February 23, 1848) was an American statesman who served as the sixth President of the United States from 1825 to 1829. He also served as a diplomat, a Senator and member of the House of Representatives. He was a member of the Federalist, Democratic-Republican, National Republican, and later Anti-Masonic and Whig parties.",
                Image=new Uri("ms-appx:///Assets/JQA_Photo_tif.jpg"),
            },
            new President()
            {
                FirstName="Andrew",
                LastName="Jackson",
                NickName="Old Hickory",
                PresidentNumber="7th president",
                YearsInOffice="1829-1837 (2 terms)",
                Party="Democratic",
                State="Tennessee",
                VP="John C. Calhoun and Martin Van Buren",
                Summary="Andrew Jackson (March 15, 1767 – June 8, 1845) was the seventh President of the United States (1829–37). He was born near the end of the colonial era, somewhere near the then-unmarked border between North and South Carolina, into a recently immigrated Scots-Irish farming family of relatively modest means.",
                Image=new Uri("ms-appx:///Assets/Andrew_Jackson_Daguerrotype-crop.jpg"),
            },
            new President()
            {
                FirstName="Martin",
                LastName="Van Buren",
                NickName="Little Magician",
                PresidentNumber="8th president",
                YearsInOffice="1837-1841",
                Party="Democratic",
                State="New York",
                VP="Richard Mentor Johnson",
                Summary="Martin Van Buren (December 5, 1782 – July 24, 1862) was an American politician who served as the eighth President of the United States (1837–41). A member of the Democratic Party, he served in a number of senior roles, including eighth Vice President (1833–37) and tenth Secretary of State (1829–31), both under Andrew Jackson.",
                Image=new Uri("ms-appx:///Assets/Martin_Van_Buren_by_Mathew_Brady_c1855-58.jpg"),
            },
            new President()
            {
                FirstName="William Henry",
                LastName="Harrison",
                NickName="General Mum",
                PresidentNumber="9th president",
                YearsInOffice="1841",
                Party="Whig",
                State="Ohio",
                VP="John Tyler",
                Summary="William Henry Harrison (February 9, 1773 – April 4, 1841) was the ninth President of the United States (1841), an American military officer and politician, and the last President born as a British subject. He was also the first president to die in office. He was 68 years, 23 days old when inaugurated, the oldest president to take office until Ronald Reagan in 1981.",
                Image=new Uri("ms-appx:///Assets/165px-William_Henry_Harrison_daguerreotype_edit.jpg"),
            },
            new President()
            {
                FirstName="John",
                LastName="Tyler",
                NickName="His Accidency",
                PresidentNumber="10th president",
                YearsInOffice="1841-1845",
                Party="Whig and Independent",
                State="Virginia",
                VP="Vacant",
                Summary="John Tyler (March 29, 1790 – January 18, 1862) was the tenth President of the United States (1841–45). He was elected vice president on the 1840 Whig ticket with William Henry Harrison, and became president after his running mate's death in April 1841.",
                Image=new Uri("ms-appx:///Assets/Tyler_Daguerreotype_(restoration).jpg"),
            },


            new President()
            {
                FirstName="James K.",
                LastName="Polk",
                NickName="Young Hickory",
                PresidentNumber="11th president",
                YearsInOffice="1845-1849 (1 term)",
                Party="Democrat",
                State="Tennessee",
                VP="George M. Dallas",
                Summary="James Knox Polk was the only president to have served as house speaker, and fulfilled all of his policies set during his campaign. He lead the sweeping victory in the Mexican-American war, seizing what is now the American Southwest.",
                Image=new Uri("ms-appx:///Assets/JamesKPolk.jpg"),
            },

new President()
            {
                FirstName="Zachary",
                LastName="Taylor",
                NickName="Old Rough and Ready",
                PresidentNumber="12th president",
                YearsInOffice="1849-1850 (<1 term)",
                Party="Whig",
                State="Tennessee",
                VP="Millard Fillmore",
                Summary="Zachary Taylor won the presidency despite his lack of interest in politics, it's believed that his status as a national war hero in the Mexican-American war won him his election. He died 17 months into his election, due to an \"unknown digestive ailment\".",
                Image=new Uri("ms-appx:///Assets/Zachary_Taylor.png"),
            },

new President()
            {
                FirstName="Millard",
                LastName="Fillmore",
                PresidentNumber="13th president",
                YearsInOffice="1850-1853 (1 term)",
                Party="Whig",
                State="New York",
                VP="None",
                Summary="Millard Fillmore was the last \"Whig\" president, meaning he was not associated with either the Republican or Democrat party. When the Whig party broke up in 1854-1856 (after his term), Millard refused to join any party.",
                Image=new Uri("ms-appx:///Assets/Millard_Fillmore_by_Brady_Studio.jpg"),
            },

new President()
            {
                FirstName="Franklin",
                LastName="Pierce",
                PresidentNumber="14th president",
                YearsInOffice="1853-1857 (1 term)",
                Party="Democrat",
                State="New Hampshire",
                VP="William R. King (1853), None (1853-1857)",
                Summary="Franklin Pierce disliked the abolitionist movement (the movement to end slavery in America) and in signing the Kansas-Nebraska Act set the stage for the attempted Southern secession. He was seen as a \"compromise\" candidate by the Democrats, and was not elected for a second term.",
                Image=new Uri("ms-appx:///Assets/Franklin_Pierce_MathewBrady.jpg"),
            },

new President()
            {
                FirstName="James",
                LastName="Buchanan",
                NickName="Doughface",
                PresidentNumber="15th president",
                YearsInOffice="1857-1861 (1 term)",
                Party="Democrat",
                State="Pennsylvania",
                VP="John C. Breckinridge",
                Summary="James Buchanan Jr. served as president immediately before the Civil War. It is believed that his efforts to maintain peace between the North and South resulted in only alienating both sides even more. He desired to be ranked in history among George Washington were unfulfilled due to his inability to find peace between the North and South.",
                Image=new Uri("ms-appx:///Assets/James_Buchanan.jpg"),
            },

new President()
            {
                FirstName="Abraham",
                LastName="Lincoln",
                NickName="Honest Abe",
                PresidentNumber="16th president",
                YearsInOffice="1861-1865 (1 term)",
                Party="Republican",
                State="Illinois",
                VP="Hannibal Hamlin (1861-1865), Andrew Johnson (1865)",
                Summary="Abraham Lincoln served only 1 term before his infamous assassination on April, 1865. He led the country through the Civil War and managed to preserve the Union and abolish slavery during that time. He was mainly self-educated and was originally a member of the Whig party, before becoming Republican in 1854.",
                Image=new Uri("ms-appx:///Assets/Abraham_Lincoln.jpg"),
            },

new President()
            {
                FirstName="Andrew",
                LastName="Johnson",
                PresidentNumber="17th president",
                YearsInOffice="1865-1869 (1 term)",
                Party="Democrat",
                State="Tennessee",
                VP="None",
                Summary="Andrew Johnson became president through the face that he was vice president at the time of Abraham Lincoln's assassination. He came into conflict with the largely Republican dominated Congress, and was the first president to be impeached after 1 term.",
                Image=new Uri("ms-appx:///Assets/Andrew_Johnson.jpg"),
            },

new President()
            {
                FirstName="Ulysses S.",
                LastName="Grant",
                NickName="Sam",
                PresidentNumber="18th president",
                YearsInOffice="1869-1877 (2 term)",
                Party="Republican",
                State="Ohio",
                VP="Schuyler Colfax (1869-1873), Henry Wilson (1873-1875), None (1875-1877)",
                Summary="Ulysses S. Grant worked closly with former president Abraham Lincoln during the Civil War to help him lead the Union Army to victory over the Confederacy. He struggled financially in civilian life and has been criticized for corruption and leading the nation into a severe economic depression during his second term.",
                Image=new Uri("ms-appx:///Assets/Ulysses_Grant.jpg"),
            },

new President()
            {
                FirstName="Rutherford B.",
                LastName="Hayes",
                PresidentNumber="19th president",
                YearsInOffice="1877-1881 (1 term)",
                Party="Republican",
                State="Ohio",
                VP="William Wheeler",
                Summary="Rutherford B. Hayes had strong beliefs in meritocratic government, equal treatment without regard to race, and improvement through education. During his presidency he oversaw the end of the Reconstruction (started by Ulysses Grant), and began to lead the nation to civil service reform. He was also left (and attempted) to reconcile the divisions left over from the Civil War.",
                Image=new Uri("ms-appx:///Assets/Rutherford_Hayes_MathewBrady.jpg"),
            },


            new President()
            {
                FirstName="James",
                LastName="Garfield",
                NickName=" ",
                PresidentNumber="20th president",
                YearsInOffice="1881-1881 (assassinated)",
                Party="Republican",
                State="Ohio",
                VP="Chester A. Arthur",
                Summary="James Abram Garfield (November 19, 1831 – September 19, 1881) was the 20th President of the United States, serving from March 4, 1881, until his assassination later that year. Garfield had served nine terms in the House of Representatives, and had been elected to the Senate before his candidacy for the White House, though he declined the senatorship once he was president-elect. He is the only sitting House member to be elected president.",
                Image=new Uri("ms-appx:///Assets/garfield20.jpg"),
            },
            new President()
            {
                FirstName="Chester",
                LastName="Arthur",
                NickName=" ",
                PresidentNumber="21st president",
                YearsInOffice="1881-1885 (1 term)",
                Party="Republican",
                State="New York",
                VP="Vacant",
                Summary="Chester Alan Arthur (October 5, 1829 – November 18, 1886) was an American attorney and politician who served as the 21st President of the United States (1881–85); he succeeded James A. Garfield upon the latter's assassination. At the outset, Arthur struggled to overcome a slightly negative reputation, which stemmed from his early career in politics as part of New York's Republican political machine. He succeeded by embracing the cause of civil service reform. His advocacy for, and subsequent enforcement of, the Pendleton Civil Service Reform Act was the centerpiece of his administration.",
                Image=new Uri("ms-appx:///Assets/arthur21.jpg"),
            },
            new President()
            {
                FirstName="Grover",
                LastName="Cleveland",
                NickName=" ",
                PresidentNumber="22nd president",
                YearsInOffice="1885-1889 (1st term)",
                Party="Democrat",
                State="New York",
                VP="Thomas A. Hendricks",
                Summary="Stephen Grover Cleveland (March 18, 1837 – June 24, 1908) was the 22nd and 24th President of the United States.  He was the winner of the popular vote for president three times – in 1884, 1888, and 1892 – and was one of the three Democrats (with Andrew Johnson and Woodrow Wilson) to serve as president during the era of Republican political domination dating from 1861 to 1933.",
                Image=new Uri("ms-appx:///Assets/grover22.png"),
            },
            new President()
            {
                FirstName="Benjamin",
                LastName="Harrison",
                NickName=" ",
                PresidentNumber="23rd president",
                YearsInOffice="1889-1893 (1 term)",
                Party="Republican",
                State="Indiana",
                VP="Levi P. Morton",
                Summary="Benjamin Harrison (August 20, 1833 – March 13, 1901) was the 23rd President of the United States (1889–93); he was the grandson of the ninth President, William Henry Harrison. Before ascending to the presidency, Harrison established himself as a prominent local attorney, Presbyterian church leader and politician in Indianapolis, Indiana. During the American Civil War, he served the Union as a colonel and on February 14, 1865 was confirmed by the U.S. Senate as a brevet brigadier general of volunteers to rank from January 23, 1865. After the war, he unsuccessfully ran for the governorship of Indiana. He was later elected to the U.S. Senate by the Indiana legislature.",
                Image=new Uri("ms-appx:///Assets/harrison23.jpg"),
            },
            new President()
            {
                FirstName="Grover",
                LastName="Cleveland",
                NickName=" ",
                PresidentNumber="24th president",
                YearsInOffice="1893-1987 (2nd term)",
                Party="Democrat",
                State="New York",
                VP="Adlai Stevenson",
                Summary="Stephen Grover Cleveland (March 18, 1837 – June 24, 1908) was the 22nd and 24th President of the United States.  He was the winner of the popular vote for president three times – in 1884, 1888, and 1892 – and was one of the three Democrats (with Andrew Johnson and Woodrow Wilson) to serve as president during the era of Republican political domination dating from 1861 to 1933.",
                Image=new Uri("ms-appx:///Assets/grover24.jpg"),
            },
            new President()
            {
                FirstName="William",
                LastName="McKinley",
                NickName=" ",
                PresidentNumber="25th president",
                YearsInOffice="1897-1901 (1 term)",
                Party="Republican",
                State="Ohio",
                VP="Theodore Roosevelt",
                Summary="William McKinley (January 29, 1843 – September 14, 1901) was the 25th President of the United States, serving from March 4, 1897, until his assassination in September 1901, six months into his second term. McKinley led the nation to victory in the Spanish–American War, raised protective tariffs to promote American industry, and maintained the nation on the gold standard in a rejection of inflationary proposals.",
                Image=new Uri("ms-appx:///Assets/mckinley25.jpg"),
            },
            new President()
            {
                FirstName="Theodore",
                LastName="Roosevelt",
                NickName=" ",
                PresidentNumber="26th president",
                YearsInOffice="1901-1909 (2 terms)",
                Party="Republican",
                State="New York",
                VP="Charles W. Fairbanks",
                Summary="Theodore Roosevelt (October 27, 1858 – January 6, 1919) was an American statesman, author, explorer, soldier, naturalist, and reformer who served as the 26th President of the United States from 1901 to 1909. As a leader of the Republican Party during this time, he became a driving force for the Progressive Era in the United States in the early 20th century.",
                Image=new Uri("ms-appx:///Assets/roosevelt26.jpg"),
            },





            new President()

            {

                FirstName="William",

                LastName="Taft",

                PresidentNumber="27th president",

                YearsInOffice="1909–1913 (1 term)",

                Party="Republican",

                State="Ohio",

                VP="James S. Sherman:- March 4, 1909 – October 30, 1912, Vacant:-October 30, 1912 – March 4, 1913",

                Summary="William Howard Taft (September 15, 1857 – March 8, 1930) served as the 27th President of the United States (1909–1913) and as the 10th Chief Justice of the United States Supreme Court (1921–1930), the only person to have held both offices.",

                Image=new Uri("ms-appx:///Assets/William_Howard_Taft_1909.jpg"),

            },


            new President()

            {

                FirstName="Woodrow",

                LastName="Wilson",

                PresidentNumber="28th president",

                YearsInOffice="1913-1921",

                Party="Democratic",

                State="New Jersey",

                VP="Thomas R.Marshall",

                Summary="Thomas Woodrow Wilson, known as Woodrow Wilson (December 28, 1856 – February 3, 1924), was an American politician and academic who served as the 28th President of the United States from 1913 to 1921.",

                Image=new Uri("ms-appx:///Assets/President_Wilson_1919_tif.jpg"),

            },



            new President()

            {

                FirstName="Warren",

                LastName="Harding",

                PresidentNumber="29th president",

                YearsInOffice="1921-1923",

                Party="Republican",

                State="Ohio",

                VP="Calvin Coolidge",

                Summary="Warren Gamaliel Harding (November 2, 1865 – August 2, 1923) was the 29th President of the United States, serving from March 4, 1921 until his death.",

                Image=new Uri("ms-appx:///Assets/Warren_G_Harding-Harris_&_Ewing.jpg"),

            },



            new President()

            {

                FirstName="Calvin",

                LastName="Coolidge",

                PresidentNumber="30th president",

                YearsInOffice="1923-1929",

                Party="Republican",

                State="Massachusetts",

                VP="Charles.D.Dawes",

                Summary="John Calvin Coolidge Jr. (July 4, 1872 – January 5, 1933) was the 30th President of the United States (1923–29).",

                Image=new Uri("ms-appx:///Assets/Calvin_Coolidge_cph_3g10777.jpg"),

            },



            new President()

            {

                FirstName="Herbert",

                LastName="Hoover",

                NickName="The Great Engineer",

                PresidentNumber="31st president",

                YearsInOffice="1929-1933",

                Party="Republican",

                State="Iowa",

                VP="Charles Curtis",

                Summary="Herbert Clark Hoover (August 10, 1874 – October 20, 1964) was the 31st President of the United States (1929–33).",

                Image=new Uri("ms-appx:///Assets/President_Hoover_portrait_tif.jpg"),

            },



            new President()

            {

                FirstName="Franklin",

                LastName="Roosevelt",

                NickName="FDR",

                PresidentNumber="32nd president",

                YearsInOffice="1933-1945",

                Party="Democratic",

                State="New York",

                VP="John Nance Garner March 4, 1933 – January 20, 1941, Henry A. Wallace January 20, 1941 – January 20, 1945, Harry S. Truman January 20, 1945 – April 12, 1945",

                Summary="Franklin Delano Roosevelt (January 30, 1882 – April 12, 1945), commonly known as FDR, was an American statesman and political leader who served as the President of the United States from 1933 to 1945.",

                Image=new Uri("ms-appx:///Assets/Franklin_D__Roosevelt.jpg"),

            },



    new President()

            {

                FirstName="Harry",

                LastName="Truman",

                PresidentNumber="33rd president",

                YearsInOffice="1945-1953",

                Party="Democratic",

                State="Missouri",

                VP="John Nance Garner March 4, 1933 – January 20, 1941, Henry A. Wallace January 20, 1941 – January 20, 1945, Harry S. Truman January 20, 1945 – April 12, 1945",

                Summary="Harry S. Truman(May 8, 1884 – December 26, 1972) was the 33rd President of the United States (1945–53), an American politician of the Democratic Party.",

                Image=new Uri("ms-appx:///Assets/Harry_S__Truman.jpg"),

            },



    new President()

            {

                FirstName="Dwight",

                LastName="Eisenhover",

                NickName="Ike",

                PresidentNumber="34th president",

                YearsInOffice="1953-1961",

                Party="Republican",

                State="Kansas",

                VP="Richard Nixon",

                Summary="Dwight David \"Ike\" Eisenhower (October 14, 1890 – March 28, 1969) was the 34th President of the United States from 1953 until 1961.",

                Image=new Uri("ms-appx:///Assets/President_Eisenhower.jpg"),

            },



    new President()

            {

                FirstName="John",

                LastName="Kennedy",

                NickName="JFK",

                PresidentNumber="34th president",

                YearsInOffice="1961-1963",

                Party="Democratic",

                State="Massachusetts",

                VP="Lyndon.B. Johnson",

                Summary="John Fitzgerald \"Jack\" Kennedy (May 29, 1917 – November 22, 1963), commonly referred to by his initials JFK, was an American politician who served as the 35th President of the United States from January 1961 until his assassination in November 1963.",

                Image=new Uri("ms-appx:///Assets/John_F__Kennedy.jpg"),

            },






            new President()
 {
 FirstName="Lyndon B.",
 LastName="Johnson",
 NickName="Light-Bulb Lyndon",
 PresidentNumber="36th president",
 YearsInOffice="1963-1969 (1.5 terms)",
 Party="Democratic",
 State="Texas",
 VP="Hubert Humphrey",
 Summary="Lyndon Baines Johnson, often referred to as LBJ, was the 36th President of the United States from 1963 to 1969, assuming the office after serving as the 37th Vice President of the United States from 1961 to 1963. Johnson was a Democrat from Texas, who served as a United States Representative from 1937 to 1949 and as a United States Senator from 1949 to 1961. He spent six years as Senate Majority Leader, two as Senate Minority Leader, and two as Senate Majority Whip.",
 Image=new Uri("ms-appx:///Assets/Lyndon_B._Johnson_Oval_Office_Portrait.jpg"),
 },
new President()
 {
 FirstName="Richard",
 LastName="Nixon",
 NickName="Tricky Dick",
 PresidentNumber="37th president",
 YearsInOffice="1969-1974 (1.5 terms)",
 Party="Republican",
 State="California",
 VP="Spiro Agnew, Gerald Ford",
 Summary="Richard Milhous Nixon was the 37th President of the United States, serving from 1969 to 1974 when he became the only U.S. president to resign the office. Nixon had previously served as a U.S. Representative and Senator from California and as the 36th Vice President of the United States from 1953 to 1961.",
 Image=new Uri("ms-appx:///Assets/Richard_M._Nixon_NARA_-_530679.jpg"),
 },
new President()
 {
 FirstName="Gerald",
 LastName="Ford",
 NickName="Mr. Nice Guy",
 PresidentNumber="38th president",
 YearsInOffice="1974-1977 (1 term)",
 Party="Republican",
 State="Michigan",
 VP="Nelson Rockefeller",
 Summary="Gerald Rudolph Ford Jr. was an American politician who served as the 38th President of the United States from 1974 to 1977. Prior to this he was the 40th Vice President of the United States, serving from 1973 until President Richard Nixon's resignation in 1974. He was the first person appointed to the vice presidency under the terms of the 25th Amendment, following the resignation of Vice President Spiro Agnew on October 10, 1973. Becoming president upon Richard Nixon's departure on August 9, 1974, he claimed the distinction as the first and to date the only person to have served as both Vice President and President of the United States without being elected to either office. Before ascending to the vice presidency, Ford served 25 years as Representative from Michigan's 5th congressional district, the final 9 of them as the House Minority Leader.",
 Image=new Uri("ms-appx:///Assets/Gerald_Ford_-_NARA_-_530680.jpg"),
 },
new President()
 {
 FirstName="Jimmy",
 LastName="Carter",
 NickName="The Peanut Farmer",
 PresidentNumber="39th president",
 YearsInOffice="1977-1981 (1 term)",
 Party="Democratic",
 State="Georgia",
 VP="Walter Mondale",
 Summary="James Earl \"Jimmy\" Carter, Jr. is an American politician and author who served as the 39th President of the United States from 1977 to 1981. In 2002, he was awarded the Nobel Peace Prize for his work with the Carter Center.",
 Image=new Uri("ms-appx:///Assets/James_Earl__Jimmy__Carter_-_NARA_-_558522.jpg"),
 },
new President()
 {
 FirstName="Ronald",
 LastName="Reagan",
 NickName="The Great Communicator",
 PresidentNumber="40th president",
 YearsInOffice="1981-1989 (2 terms)",
 Party="Republican",
 State="California",
 VP="George H. W. Bush",
 Summary="Ronald Wilson Reagan was an American politician and actor, who served as the 40th President of the United States from 1981 to 1989. Prior to his presidency, he served as the 33rd Governor of California from 1967 to 1975, following a career as a Hollywood actor and union leader.",
 Image=new Uri("ms-appx:///Assets/Official_Portrait_of_President_Reagan_1981.jpg"),
 },
new President()
 {
 FirstName="George H. W.",
 LastName="Bush",
 NickName="Poppy",
 PresidentNumber="41st president",
 YearsInOffice="1989-1993 (1 term)",
 Party="Republican",
 State="Texas",
 VP="Dan Quayle",
 Summary="George Herbert Walker Bush is an American politician who served as the 41st President of the United States (1989–93), and the 43rd Vice President of the United States (1981–89). A Republican, he previously served as a congressman, an ambassador, and Director of Central Intelligence. He is the oldest living former President and Vice President. He is also the last living former President who is a veteran of World War II. Bush is often referred to as \"George H. W. Bush\", \"Bush 41\", \"Bush the Elder\", or \"George Bush Sr.\" to distinguish him from his eldest son, George W. Bush, who was the 43rd President of the United States. Prior to his son's presidency, he was known simply as George Bush or President Bush.",
 Image=new Uri("ms-appx:///Assets/George_H._W._Bush_President_of_the_United_States_1989_official_portrait.jpg"),
 },
new President()
 {
 FirstName="Bill",
 LastName="Clinton",
 NickName="Bubba",
 PresidentNumber="42nd president",
 YearsInOffice="1993-2001 (2 terms)",
 Party="Democratic",
 State="Arkansas",
 VP="Al Gore",
 Summary="William Jefferson \"Bill\" Clinton is an American politician who served as the 42nd President of the United States from 1993 to 2001. He previously served as Governor of Arkansas from 1979 to 1981 and 1983 to 1992, and as the state's Attorney General from 1977 to 1979. A member of the Democratic Party, ideologically Clinton was a New Democrat, and many of his policies reflected a centrist Third Way philosophy of governance.",
 Image=new Uri("ms-appx:///Assets/44_Bill_Clinton_3x4.jpg"),
 },
new President()
 {
 FirstName="George W.",
 LastName="Bush",
 NickName="Bush Jr.",
 PresidentNumber="43rd president",
 YearsInOffice="2001-2009 (2 terms)",
 Party="Republican",
 State="Texas",
 VP="Dick Cheney",
 Summary="George Walker Bush is an American politician and businessman who served as the 43rd President of the United States from 2001 to 2009, and the 46th Governor of Texas from 1995 to 2000. The eldest son of Barbara and George H. W. Bush, he was born in New Haven, Connecticut. After graduating from Yale University in 1968 and Harvard Business School in 1975, he worked in oil businesses. He married Laura Welch in 1977 and ran unsuccessfully for the House of Representatives shortly thereafter. He later co-owned the Texas Rangers baseball team before defeating Ann Richards in the 1994 Texas gubernatorial election. He was elected president in 2000 after a close and controversial election, becoming the fourth president to be elected while receiving fewer popular votes nationwide than his opponent.[6] He is the second president to have been the son of a former president, the first having been John Quincy Adams.[7] He is also the brother of Jeb Bush, a former Governor of Florida and former candidate for the Republican presidential nomination in the 2016 presidential election.",
 Image=new Uri("ms-appx:///Assets/George-W-Bush.jpg"),
 },
new President()
 {
 FirstName="Barack",
 LastName="Obama",
 PresidentNumber="44th president",
 YearsInOffice="2009-2016 (2 terms)",
 Party="Democratic",
 State="Illinois",
 VP="Joe Biden",
 Summary="Barack Hussein Obama II is an American politician serving as the 44th President of the United States, the first African American to hold the office. Born in Honolulu, Hawaii, Obama is a graduate of Columbia University and Harvard Law School, where he served as president of the Harvard Law Review. He was a community organizer in Chicago before earning his law degree. He worked as a civil rights attorney and taught constitutional law at University of Chicago Law School between 1992 and 2004. He served three terms representing the 13th District in the Illinois Senate from 1997 to 2004, and ran unsuccessfully in the Democratic primary for the United States House of Representatives in 2000 against incumbent Bobby Rush.",
 Image=new Uri("ms-appx:///Assets/President_Barack_Obama.jpg"),
},
        };

        public static ObservableCollection<President> FilteredPresidents = new ObservableCollection<President>();

        public static void SetFilteredPresidents(IEnumerable<President> i)
        {
            FilteredPresidents.Clear();
            foreach (var p in i)
            {
                FilteredPresidents.Add(p);
            }
        }
        static President()
        {
            SetFilteredPresidents(AllPresidents);
        }
    }
}
