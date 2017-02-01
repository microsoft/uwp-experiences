using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacecraftExplorer
{
    public static class ShuttleHelper
    {
        public static string GetHighlightComponent()
        {
            string component = "";
            switch (ShuttleExplorer.GetHighlightID())
            {
                case 0:
                    component = "SOLID ROCKET BOOSTER I";
                    ShuttleAttitudeControl.SetRotateX(0.0f);
                    ShuttleAttitudeControl.SetRotateY(220.0f);
                    ShuttleAttitudeControl.SetRotateZ(0.0f);
                    break;

                case 1:
                    component = "SOLID ROCKET BOOSTER II";
                    ShuttleAttitudeControl.SetRotateX(0.0f);
                    ShuttleAttitudeControl.SetRotateY(108.0f);
                    ShuttleAttitudeControl.SetRotateZ(0.0f);
                    break;

                case 2:
                    component = "EXTERNAL FUEL TANK";
                    ShuttleAttitudeControl.SetRotateX(0.0f);
                    ShuttleAttitudeControl.SetRotateY(180.0f);
                    ShuttleAttitudeControl.SetRotateZ(0.0f);
                    break;

                case 3:
                    component = "SPACE SHUTTLE";
                    ShuttleAttitudeControl.SetRotateX(0.0f);
                    ShuttleAttitudeControl.SetRotateY(0.0f);
                    ShuttleAttitudeControl.SetRotateZ(0.0f);
                    break;

                case 4:
                    component = "SSME";
                    ShuttleAttitudeControl.SetRotateX(55.0f);
                    ShuttleAttitudeControl.SetRotateY(0.0f);
                    ShuttleAttitudeControl.SetRotateZ(10.0f);
                    break;

                case 5:
                    component = "BAY DOOR I";
                    ShuttleAttitudeControl.SetRotateX(0.0f);
                    ShuttleAttitudeControl.SetRotateY(330.0f);
                    ShuttleAttitudeControl.SetRotateZ(0.0f);
                    break;

                case 6:
                    component = "BAY DOOR II";
                    ShuttleAttitudeControl.SetRotateX(0.0f);
                    ShuttleAttitudeControl.SetRotateY(33.0f);
                    ShuttleAttitudeControl.SetRotateZ(0.0f);
                    break;
                default:
                    component = "MISSION INFO";
                    ShuttleAttitudeControl.SetRotateX(0.0f);
                    ShuttleAttitudeControl.SetRotateY(0.0f);
                    ShuttleAttitudeControl.SetRotateZ(0.0f);
                    break;
            }
            return component;
        }


        public static string ComponentInfo(int id)
        {
            string info = "The Space Shuttle was the first operational orbital spacecraft designed for reuse. It carried different payloads to low Earth orbit, provided crew rotation and supplies for the International Space Station (ISS), and performed satellite servicing and repair. The orbiter could also recover satellites and other payloads from orbit and return them to Earth. Each Shuttle was designed for a projected lifespan of 100 launches or ten years of operational life, although this was later extended. The person in charge of designing the STS was Maxime Faget, who had also overseen the Mercury, Gemini, and Apollo spacecraft designs. The crucial factor in the size and shape of the Shuttle orbiter was the requirement that it be able to accommodate the largest planned commercial and military satellites, and have over 1,000 mile cross-range recovery range to meet the requirement for classified USAF missions for a once-around abort from a launch to a polar orbit.";

            switch (ShuttleExplorer.GetHighlightID())
            {
                case -1:
                    break;
                case 0:
                    info = "The Space Shuttle Solid Rocket Boosters (SRBs) were the first solid fuel motors to be used for primary propulsion on a vehicle used for human spaceflight and provided the majority of the Space Shuttle's thrust during the first two minutes of flight. After burnout, they were jettisoned and parachuted into the Atlantic Ocean where they were recovered, examined, refurbished, and reused.";
                    break;

                case 1:
                    info = "The Space Shuttle Solid Rocket Boosters (SRBs) were the first solid fuel motors to be used for primary propulsion on a vehicle used for human spaceflight[1] and provided the majority of the Space Shuttle's thrust during the first two minutes of flight. After burnout, they were jettisoned and parachuted into the Atlantic Ocean where they were recovered, examined, refurbished, and reused.";
                    break;

                case 2:
                    info = "A Space Shuttle External Tank (ET) was the component of the Space Shuttle launch vehicle that contained the liquid hydrogen fuel and liquid oxygen oxidizer. During lift-off and ascent it supplied the fuel and oxidizer under pressure to the three Space Shuttle Main Engines (SSME) in the orbiter. The ET was jettisoned just over 10 seconds after MECO (Main Engine Cut Off), where the SSMEs were shut down, and re-entered the Earth's atmosphere. Unlike the Solid Rocket Boosters, external tanks were not re-used. They broke up before impact in the Indian Ocean (or Pacific Ocean in the case of direct-insertion launch trajectories), away from shipping lanes and were not recovered.";
                    break;

                case 3:
                    info = "The Space Shuttle was a partially reusable low Earth orbital spacecraft system operated by the U.S. National Aeronautics and Space Administration (NASA), as part of the Space Shuttle program. Its official program name was Space Transportation System (STS), taken from a 1969 plan for a system of reusable spacecraft of which it was the only item funded for development.";
                    break;

                case 4:
                    info = "The Aerojet Rocketdyne RS-25, otherwise known as the Space Shuttle main engine (SSME), is a liquid-fuel cryogenic rocket engine that was used on NASA's Space Shuttle and is planned to be used on its successor, the Space Launch System. Built in the United States by Rocketdyne, the RS-25 burns cryogenic liquid hydrogen and liquid oxygen propellants, with each engine producing 1,859 kN (418,000 lbf) of thrust at liftoff.";
                    break;

                case 5:
                    info = "The orbiter carried its payload in a large cargo bay with doors that opened along the length of its top, a feature which made the Space Shuttle unique among spacecraft. This feature made possible the deployment of large satellites such as the Hubble Space Telescope and also the capture and return of large payloads back to Earth.";
                    break;

                case 6:
                    info = "The orbiter carried its payload in a large cargo bay with doors that opened along the length of its top, a feature which made the Space Shuttle unique among spacecraft. This feature made possible the deployment of large satellites such as the Hubble Space Telescope and also the capture and return of large payloads back to Earth.";
                    break;
            }

            return info;
        }


        public static string GetMissionStage()
        {
            string stage = "STARTUP";
            if (ShuttleSimulator.currentNormalizedTime >= 0.1f)
            {
                stage = "IGNITION";
            }
            if (ShuttleSimulator.currentNormalizedTime >= 0.13f)
            {
                stage = "STARTUP";
            }
            if (ShuttleSimulator.currentNormalizedTime >= 0.22f)
            {
                stage = "MAX Q";
            }
            if (ShuttleSimulator.currentNormalizedTime >= 0.33f)
            {
                stage = "SRB SEPARATION";
            }
            if (ShuttleSimulator.currentNormalizedTime >= 0.44f)
            {
                stage = "SRB IMPACT";
            }
            if (ShuttleSimulator.currentNormalizedTime >= 0.55f)
            {
                stage = "MECO";
            }
            if (ShuttleSimulator.currentNormalizedTime >= 0.66f)
            {
                stage = "ET SEPARATION";
            }
            if (ShuttleSimulator.currentNormalizedTime >= 0.77f)
            {
                stage = "OMS BURN";
            }
            if (ShuttleSimulator.currentNormalizedTime >= 0.88f)
            {
                stage = "ORBITING";
            }

            return stage;
        }




    }
}
