import { useParams } from "react-router-dom";
import { authService } from "./auth/auth.service";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useRef } from "react";
import { useNavigate } from "react-router-dom";
import { AppPaths } from "./routing/routes";

function RedirectPage(){
    const navigate = useNavigate();
    const { placeId,salt } = useParams();
    const [paramsValid, setParamsValid] = useState<boolean | null>(null);
    const [passCodeRequired, setPassCodeRequired] = useState<boolean>(false);
    const [passcodeInputValue, setPasscodeInputValue] = useState<string>("");
    const [message, setMessage] = useState<string>("");
    const { t } = useTranslation("public");
    const hasRun = useRef(false);
    
    
    const checkAndGetToken = async () => {
        if (isNaN(Number(placeId)) || salt?.length !== 32) {
            setParamsValid(false);
            return;
        }

        if(authService.getLastSessionCheckTime()){
            const secondsSinceLastCall = ((new Date()).getTime() - authService.getLastSessionCheckTime()!.getTime()) / 1000;
            if(secondsSinceLastCall < 5){ return;}
        }

        const response = await authService.getGuestToken(salt!,false);

        if (!response.isSessionEstablished) {
            setPassCodeRequired(true);
        }
        else{
            authService.setGuestToken(response.guestToken,placeId!);
            setTimeout(()=>{navigate(AppPaths.public.myOrders.replace(":placeId", placeId!));},100)
        }
            
    }

    const joinTable = async(e:any) => {
        e.preventDefault();
        if(!passcodeInputValue.trim()) return;
        let response:any = {};
        try{
            response = await authService.joinTable(passcodeInputValue,salt!);
            if (!response.isSessionEstablished) {
                setPassCodeRequired(true);
                setMessage(t("invalid_passcode_message"))
            }
            else{
                authService.setGuestToken(response.guestToken,placeId!);
                console.log("redirect redirectPage ");
                navigate(AppPaths.public.myOrders.replace(":placeId",placeId!.toString()));
            }
                
        }
        catch(error:any){
            setPassCodeRequired(true);
            setMessage(t("invalid_passcode_message"))
        }
    }
    useEffect(() => {
        if (hasRun.current) return;
        hasRun.current = true;
        checkAndGetToken();
    }, []);
    
    return (
        <div className="min-h-[80vh] flex items-center justify-center">
        <div className="flex flex-col items-center text-center text-black space-y-4">
            {passCodeRequired ? (
                <>
                    <label className="text-lg font-semibold">{t("passcode_required")}</label>
                    <input
                        type="text"
                        className="max-w-[200px] p-2 border bg-white rounded-lg"
                        required
                        onChange={(e) => setPasscodeInputValue(e.target.value)}
                    />
                    <button
                        onClick={(e) => joinTable(e)}
                        className=" px-15 py-2 rounded-[40px] text-white bg-mocha-600"
                    >
                        {t("join_table").toUpperCase()}
                    </button>
                    <p className="text-red-500">{message}</p>
                </>
            ) : (
                <>
                    <h1 className="text-xl font-medium">Loading...</h1>
                    {paramsValid === false && (
                        <h1 className="text-red-500 font-semibold">Not found!</h1>
                    )}
                </>
            )}
        </div>
    </div>
    );
    
}

export default RedirectPage;