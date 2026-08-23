import "./registerform.css";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "../../stores/authStore";

export default function RegisterForm() {
  const [error, setError] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    username: "",
    password: "",
  });

  const { createAccount } = useAuthStore();

  const navigate = useNavigate();

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    try {
      await createAccount(formData.username, formData.password);
      navigate("/");
    } catch (err: any) {
      setError(err.response.data.message);
    }
  }

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const inputClass = error ? "input error" : "input";

  return (
    <div className="sign-in-container">
      <form className="form" onSubmit={handleSubmit}>
        <div className="flex-column">
          <label>Username</label>
        </div>

        <div className={`inputForm ${error ? "error" : ""}`}>
          <input
            name="username"
            value={formData.username}
            onChange={handleChange}
            placeholder="Enter your Username"
            className={inputClass}
            type="text"
            required
          />
        </div>

        <div className="flex-column">
          <label>Password</label>
        </div>

        <div className={`inputForm ${error ? "error" : ""}`}>
          <input
            name="password"
            value={formData.password}
            onChange={handleChange}
            placeholder="Enter your Password"
            className={inputClass}
            type="password"
            required
          />
        </div>

        {error && <div className="error-message">{error}</div>}

        <div className="flex-row"></div>
        <button className="button-submit" type="submit">
          Sign Up
        </button>
        <p className="p">
          Already have an account?
          <span className="span" onClick={() => navigate(`/login`)}>
            Sign in
          </span>
        </p>
      </form>
    </div>
  );
}
