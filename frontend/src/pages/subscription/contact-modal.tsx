import { useEffect, useState } from "react";

type ContactModalParam = {
    isOpen: boolean;
    onClose: () => void;
    subject: string;
    message: string;
    t: (key: string) => string;
};

const ContactModal = ({ isOpen, onClose, subject: initialSubject, message: initialMessage, t }: ContactModalParam) => {
    const [sender, setSender] = useState("");
    const [subject, setSubject] = useState(initialSubject);
    const [message, setMessage] = useState(initialMessage);
    const [error, setError] = useState("");

    useEffect(() => {
        if (isOpen) {
            document.body.style.overflow = "hidden";
            setSender("");
            setSubject(initialSubject);
            setMessage(initialMessage);
            setError("");
        } else {
            document.body.style.overflow = "";
        }
        return () => {
            document.body.style.overflow = "";
        };
    }, [isOpen, initialSubject, initialMessage]); 

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();

        if (!sender.trim() || !subject.trim() || !message.trim()) {
            setError("Please Fill all the fields");
            return;
        }
        setError("");
        console.log("Sending form:", { sender, subject, message });
        onClose();
    };

    if (!isOpen) return null;

    return (
        <div
            onClick={onClose}
            className="fixed inset-0 z-50 flex items-center justify-center backdrop-blur-md">
            <div
                onClick={(e) => e.stopPropagation()}
                role="dialog"
                aria-modal="true"
                aria-labelledby="contact-modal-title"
                className="bg-neutral-100 border border-neutral-300 rounded-2xl p-6 px-8 w-full max-w-xl relative transform transition-all duration-300 scale-100">
                
                <div className="flex items-center justify-between mb-4">
                    <h2 id="contact-modal-title" className="text-xl font-semibold">
                        {t("contact")}
                    </h2>
                    <button
                        onClick={onClose}
                        className="text-gray-500 hover:text-black"
                        aria-label="Close Modal">
                        <img src="../../../public/assets/images/close.svg" className="w-5 h-5 text-neutral-500" alt="✕" />
                    </button>
                </div>
                <form className="p-4" onSubmit={handleSubmit}>
                <label className="block mb-2 text-base">{t("sender")}*</label>
                    <input
                        className="w-full border border-neutral-400 p-3 mb-4 rounded-xl"
                        onChange={(e) => setSender(e.target.value)}
                    />

                    <label className="block mb-2 text-base">{t("subject")}*</label>
                    <input
                        className="w-full border border-neutral-400 p-3 mb-4 rounded-xl"
                        value={subject}
                        onChange={(e) => setSubject(e.target.value)}
                    />

                    <label className="block mb-2 text-base">{t("message")}*</label>
                    <textarea
                        className="w-full border border-neutral-400 p-2 h-32 rounded-xl"
                        value={message}
                        onChange={(e) => setMessage(e.target.value)}
                    />

                    {error && <p className="text-red-500 text-sm mb-4">{error}</p>}
                    <div className="flex justify-end gap-2 mt-4">
                        <button
                            type="button"
                            onClick={onClose}
                            className="bg-neutral-200 px-4 py-2 rounded-lg w-1/2 hover:bg-neutral-300"
                        >
                            {t("close")}
                        </button>
                        <button type="submit" 
                            className="bg-blue-400 text-black px-4 py-2 rounded-lg w-1/2 hover:bg-blue-600 hover:text-white">
                            {t("send")}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default ContactModal;
