import { useState } from "react";


function AboutUs(){

    const[number, setNumber] = useState(10);

    const uvecaj = () => {
        let _newnumber = number + 1;
        setNumber(_newnumber);
    }

    return(
       <>
        <div className="about-us">
            <h2>O nama</h2>
        </div>

        <div>
            <p>
                ovo je: {number}
            </p>
            <button
                onClick={() => uvecaj()}
            >
                UVEĆAJ
            </button>
        </div>
       </>
    )
}

export default AboutUs;