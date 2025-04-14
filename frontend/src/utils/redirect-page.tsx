import { useParams } from "react-router-dom";
import { authService } from "./auth/auth.service";
import { useEffect, useState } from "react";


function RedirectPage(){

    const { placeId,salt } = useParams();
    const [paramsValid, setParamsValid] = useState<boolean | null>(null);
    console.log("redirect page ")
    const checkAndGetToken = async () => {
        if (isNaN(Number(placeId)) || salt?.length !== 32) {
            setParamsValid(false);
            console.log("nije ok ")
            return;
        }
        const response = await authService.getGuestToken(salt!);
        // if (response.isAvailable && response.guestToken) {
        if (response.guestToken) {
            console.log("settiram token")
            authService.setGuestToken(response.guestToken,placeId!);
        }
    }
    useEffect(() => {
        setTimeout(()=>{checkAndGetToken();},5000)
        
    })
    
    return (
        <div>
            <h1>Loading...</h1>
            {paramsValid === false && <h1>Not found!</h1>}
        </div>
    );
    
}

export default RedirectPage;