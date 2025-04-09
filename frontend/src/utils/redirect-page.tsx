import { useParams } from "react-router-dom";
import { authService } from "./auth/auth.service";
import { useEffect, useState } from "react";


function RedirectPage(){

    const { placeId,salt } = useParams();
    const [paramsValid, setParamsValid] = useState<boolean | null>(null);
    

    const checkAndGetToken = async () => {
        if (isNaN(Number(placeId)) || salt?.length !== 32) {
            setParamsValid(false);
            return;
        }
        const response = await authService.getGuestToken(salt!);
        if (response.isAvailable && response.token) {
            authService.setGuestToken(response.token,placeId!);
        }
    }
    useEffect(() => {
        checkAndGetToken();
    })
    
    return (
        <div>
            <h1>Loading...</h1>
            {paramsValid === false && <h1>Not found!</h1>}
        </div>
    );
    
}

export default RedirectPage;